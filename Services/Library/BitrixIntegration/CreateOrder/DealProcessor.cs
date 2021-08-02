using BitrixApi.DTO;
using BitrixApi.REST;
using NLog;
using QS.DomainModel.UoW;
using QS.Osm.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Common;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Parameters;
using Vodovoz.Repositories.Client;
using Vodovoz.Repository;
using Vodovoz.Services;
using VodovozInfrastructure.Utils;
using Contact = BitrixApi.DTO.Contact;
using Phone = Vodovoz.Domain.Contacts.Phone;
using BitrixPhone = BitrixApi.DTO.Phone;

namespace BitrixIntegration
{
	public class DealProcessor//: ICoR 
    {
	    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private bool needSetFirstOrderForCounterparty = false;

        private readonly IBitrixRestApi bitrixApi;
        private readonly Matcher matcher;
        private readonly ICounterpartyContractRepository counterpartyContractRepository;
        private readonly CounterpartyContractFactory counterpartyContractFactory;
        private readonly IMeasurementUnitsRepository measurementUnitsRepository;
        private readonly IBitrixServiceSettings bitrixServiceSettings;
		private readonly IOrderRepository orderRepository;
		private readonly ICounterpartyRepository counterpartyRepository;

		public DealProcessor(
	        IBitrixRestApi bitrixApi, 
	        Matcher matcher,
	        ICounterpartyContractRepository counterpartyContractRepository,
	        CounterpartyContractFactory counterpartyContractFactory,
	        IMeasurementUnitsRepository measurementUnitsRepository,
	        IBitrixServiceSettings bitrixServiceSettings,
			IOrderRepository orderRepository,
			ICounterpartyRepository counterpartyRepository
	        )
        {
	        this.bitrixApi = bitrixApi ?? throw new ArgumentNullException(nameof(bitrixApi));
	        this.matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
	        this.bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
			this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			this.counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			this.counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
	        this.counterpartyContractFactory = counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
	        this.measurementUnitsRepository = measurementUnitsRepository ?? throw new ArgumentNullException(nameof(measurementUnitsRepository));
	        QS.Osm.Osrm.OsrmMain.ServerUrl = bitrixServiceSettings.OsrmServiceURL;
        }

		public void ProcessDeals(DateTime date)
		{
			//using(var uow = UnitOfWorkFactory.CreateWithoutRoot()){
				var deals = bitrixApi.GetDeals(date, date);
				foreach(var deal in deals)
				{
					ProcessDeal(deal);
				}
				
			//}
		}

        private void ProcessDeal(Deal deal)
        {			
	        needSetFirstOrderForCounterparty = false;

	        logger.Info($"Обработка сделки: {deal.Id}");

	        using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				var order = orderRepository.GetOrderByBitrixId(uow, deal.Id);
				if(order != null)
				{
					logger.Info($"Обработка сделки пропущена. Для сделки №{deal.Id} уже есть существующий заказ №{order.Id}.");
					return;
				}

		        logger.Info("Обработка контрагента");
		        Counterparty counterparty = ProcessCounterparty(uow, deal);

		        DeliverySchedule deliverySchedule = null;
		        DeliveryPoint deliveryPoint = null;

		        if(!deal.IsSelfDelivery) {
			        logger.Info("Обработка точек доставки");
			        deliveryPoint = ProcessDeliveryPoint(uow, deal, counterparty);
			        deliverySchedule = DeliveryScheduleRepository.GetByBitrixId(uow, deal.DeliverySchedule);
			        if(deliverySchedule == null) {
				        throw new Exception("Не найдено время в DeliveryShedule по bitrixId");
			        }
		        }

		        logger.Info("Сборка заказа");
		        var order = ProcessOrder(
			        uow,
			        deal,
			        deliveryPoint,
			        counterparty,
			        isSmsPayment,
			        deliverySchedule
		        );

		        logger.Info("Обработка номенклатур");
		        await ProcessProducts(uow, deal, order);

		        foreach(var orderItem in order.OrderItems) {
			        uow.Save(orderItem.Nomenclature);
		        }

		        uow.Save(order);
		        uow.Commit();
	        }
        }

        private Order ProcessOrder(
	        IUnitOfWork uow,
	        Deal deal,
	        DeliveryPoint deliveryPointForOrder, 
	        Counterparty counterpartyForNewOrder,
	        bool paymentBySms,
	        DeliverySchedule schedule
	    )
        {
	        var bitrixAccaunt = uow.GetById<Employee>(bitrixServiceSettings.EmployeeForOrderCreate);
	        var newOrder = new Order()
	        {
		        UoW = uow,
		        BitrixDealId = deal.Id,
		        PaymentType = deal.GetPaymentMethod(),
		        CreateDate = deal.CreateDate,
		        DeliveryDate = deal.DeliveryDate,
		        DeliverySchedule = schedule,
		        Client = counterpartyForNewOrder,
		        DeliveryPoint = deliveryPointForOrder,
		        OrderStatus = OrderStatus.Accepted,
		        Author = bitrixAccaunt,
		        LastEditor = bitrixAccaunt,
		        LastEditedTime = DateTime.Now,
		        PaymentBySms = paymentBySms,
		        OrderPaymentStatus = deal.GetOrderPaymentStatus(),
		        SelfDelivery = deal.IsSelfDelivery(),
		        Comment = deal.Comment,
		        Trifle = deal.Trifle ?? 0,
		        BottlesReturn = deal.BottlesToReturn,
		        Contract = new CounterpartyContract(),
		        EShopOrder = (int)deal.Id,
		        OnlineOrder = deal.OrderNumber ?? null
	        };
	        if (newOrder.PaymentType == PaymentType.ByCard)
	        {
		        newOrder.PaymentByCardFrom = uow.GetById<PaymentFrom>(7);
	        }

			newOrder.UpdateOrCreateContract(uow, counterpartyContractRepository, counterpartyContractFactory);
			
	        if (needSetFirstOrderForCounterparty){
		        counterpartyForNewOrder.FirstOrder = newOrder; 
	        }
	       
	        logger.Info("-------------------------");
	        return newOrder;
        }

        private Counterparty ProcessCounterparty(IUnitOfWork uow, Deal deal)
        {
	        if(deal.ContactId != 0){
				return ProcessContact(uow, deal);
	        }

	        if(deal.CompanyId != 0){
				return ProcessCompany(uow, deal);
	        }

			throw new InvalidOperationException("Сделка не имеет ни контакта ни компании, такие сделки невозможно обработать");
        }

        private Counterparty ProcessContact(IUnitOfWork uow, Deal deal)
        {
	        logger.Info("Обработка контрагента как контакта");

	        var contact = bitrixApi.GetContact(deal.ContactId);
			if(contact == null)
			{
				throw new InvalidOperationException($"Не удалось загрузить контакт №{deal.ContactId}");
			}

			Counterparty counterparty = GetCounterpartyOrNull(uow, contact);
			if(counterparty == null)
			{
				logger.Info($"Не найден контрагент для контакта: {contact.Id} {contact.SecondName} {contact.Name} {contact.LastName} " +
						", создаем нового контрагента");
				counterparty = new Counterparty()
				{
					FullName = contact.SecondName + " " + contact.Name + " " + contact.LastName,
					Name = contact.SecondName + " " + contact.Name + " " + contact.LastName,
					BitrixId = contact.Id,
					PersonType = PersonType.natural,
					CreateDate = contact.CreatedDate,
					PaymentMethod = deal.GetPaymentMethod(),
			        IsArchive = false
				};
				AddPhonesToCounterparty(counterparty, contact.Phones);

				uow.Save(counterparty);
			}

			return counterparty;
		}

        private Counterparty ProcessCompany(IUnitOfWork uow, Deal deal)
        {
			logger.Info("Обработка контрагента как компании");

			var company = bitrixApi.GetCompany(deal.CompanyId);
			if(company == null)
			{
				throw new InvalidOperationException($"Не удалось загрузить компанию №{deal.CompanyId}");
			}

			Counterparty counterparty = GetCounterpartyOrNull(uow, company);
			if(counterparty == null)
			{
				logger.Info($"Не найден контрагент для компании: {company.Title}, создаем нового контрагента");
		        counterparty = new Counterparty()
		        {
			        FullName = company.Title,
			        Name = company.Title,
			        BitrixId = company.Id,
			        PersonType =  PersonType.legal,
			        TypeOfOwnership = NamesUtils.TryGetOrganizationType (company.Title) ?? "",
			        CreateDate = company.DateCreate,
			        PaymentMethod = deal.GetPaymentMethod(),
			        IsArchive = false
		        };
				AddPhonesToCounterparty(counterparty, company.Phones);
		        uow.Save(counterparty);
	        }

		    return counterparty;
		}

		private Counterparty GetCounterpartyOrNull(IUnitOfWork uow, Contact contact)
		{
			Counterparty counterparty = counterpartyRepository.GetCounterpartyByBitrixId(uow, contact.Id);
			if(counterparty != null)
			{
				return counterparty;
			}

			var phone = contact.Phones.First().Value;
			var digitsNumber = PhoneUtils.ToDigitNumberWithoutCountryCode(phone);

			string contactName;
			if(!string.IsNullOrWhiteSpace(contact.LastName))
			{
				contactName = contact.LastName;
			}
			else if(!string.IsNullOrWhiteSpace(contact.Name))
			{
				contactName = contact.Name;
			}
			else if(!string.IsNullOrWhiteSpace(contact.SecondName))
			{
				contactName = contact.SecondName;
			}
			else
			{
				throw new InvalidOperationException("Контакт не содержит имени, фамилии и отчества, необходимых для поиска контрагента");
			}

			IList<Counterparty> counterparties = counterpartyRepository.GetCounterpartiesByNameAndPhone(uow, contactName, digitsNumber);
			var count = counterparties.Count;
			if(count == 1)
			{
				counterparty = counterparties.First();
				logger.Info($"Для контакта с BitrixId {contact.Id} у нас найден 1 контрагент {counterparty.Id} по телефону и части имени");
				return counterparty;
			}
			else if(count > 1)
			{
				var ids = counterparties.Select(x => x.Id);
				var counterpartyIds = string.Join(", ", ids);
				logger.Info($"Для контакта с BitrixId {contact.Id} найдено несколько контрагентов ({counterpartyIds}) " +
					$"по телефону и части имени. Невозможно выбрать кого-то одного");
				return null;
			}
			else
			{
				logger.Info($"Для контакта с BitrixId {contact.Id} не найдено контрагентов по телефону и части имени");
				return null;
			}
		}

		private Counterparty GetCounterpartyOrNull(IUnitOfWork uow, Company company)
		{
			Counterparty counterparty = counterpartyRepository.GetCounterpartyByBitrixId(uow, company.Id);
			if(counterparty != null)
			{
				return counterparty;
			}

			var phone = company.Phones.First().Value;
			var digitsNumber = PhoneUtils.ToDigitNumberWithoutCountryCode(phone);

			if(string.IsNullOrWhiteSpace(company.Title))
			{
				throw new InvalidOperationException("Компания не содержит названия, необходимого для поиска контрагента");
			}

			IList<Counterparty> counterparties = counterpartyRepository.GetCounterpartiesByNameAndPhone(uow, company.Title, digitsNumber);
			var count = counterparties.Count;
			if(count == 1)
			{
				counterparty = counterparties.First();
				logger.Info($"Для компании с BitrixId {company.Id} у нас найден 1 контрагент {counterparty.Id} по телефону и названию");
				return counterparty;
			}
			else if(count > 1)
			{
				var ids = counterparties.Select(x => x.Id);
				var counterpartyIds = string.Join(", ", ids);
				logger.Info($"Для компании с BitrixId {company.Id} найдено несколько контрагентов ({counterpartyIds}) " +
					$"по телефону и названию. Невозможно выбрать кого-то одного");
				return null;
			}
			else
			{
				logger.Info($"Для компании с BitrixId {company.Id} не найдено контрагентов по телефону и названию");
				return null;
			}
		}

		private void AddPhonesToCounterparty(Counterparty counterparty, IEnumerable<BitrixPhone> phones)
        {
	        foreach (var contactPhone in phones)
			{
				var phone = new Phone
				{
					Number = contactPhone.Value
				};
		        logger.Info($"Добавляем телефон: {contactPhone} контрагенту {counterparty.FullName}");
				counterparty.Phones.Add(phone);
	        }
        }

		
        private DeliveryPoint ProcessDeliveryPoint(IUnitOfWork uow, Deal deal, Counterparty counterparty)
        {
	        logger.Info("Обработка точки доставки");
	        if(counterparty == null)
			{
				throw new ArgumentNullException(nameof(counterparty));
			}

			logger.Info($"Сопоставление точки доставки: {deal.DeliveryAddressWithoutHouse} " +
	                    $"для клиента {counterparty?.FullName}");
	        
	        if (matcher.MatchDeliveryPoint(uow, deal, counterparty, out var deliveryPoint) ){
		        
		        //Если к нам пришел незаполненный адресами контрпати, то привязываем ему адрес
		        if (counterparty?.DeliveryPoints.Count == 0){
			        counterparty.DeliveryPoints.Add(deliveryPoint);
		        }
		        logger.Info("-------EndProcessDeliveryPoint-------");
		        return deliveryPoint;
	        }
	        else{
		        logger.Info("Создание новой сущности DeliveryPoint");

		        var newDeliveryPoint = DeliveryPoint.Create(counterparty);
		        
		        try{
			        newDeliveryPoint.RoomType = deal.GetRoomType();
		        }
		        catch (Exception e){
			        logger.Warn(e.Message);
			        logger.Warn($"Для заказа {deal.Id} не получилось сопоставить тип помещения '{deal.RoomType}' " +
			                    "будет выставлен тип по умолчанию: помещение");
			        newDeliveryPoint.RoomType = RoomType.Room;
		        }
		        
		        newDeliveryPoint.Room = deal.RoomNumber;

		        try{
			        newDeliveryPoint.Building = deal.GetBuilding();
		        }
		        catch (Exception e){
			        logger.Warn($"Из заказа {deal.Id} не получилось получить номер дома '{deal.HouseAndBuilding}'");
			        logger.Warn(e.Message);
		        }
		        newDeliveryPoint.Entrance = string.IsNullOrWhiteSpace(deal.Entrance)? "" : deal.Entrance;
		        newDeliveryPoint.Floor = string.IsNullOrWhiteSpace(deal.Floor)? "" : deal.Floor;
		        try{
			        newDeliveryPoint.EntranceType = deal.GetEntranceType();
		        }
		        catch (Exception e){
			        logger.Warn($"Подтип объекта(EntranceType) не выставлен");
		        }
		        newDeliveryPoint.City = deal.City;
		        logger.Info($"street: {deal.DeliveryAddressWithoutHouse}");
		        newDeliveryPoint.Street = deal.DeliveryAddressWithoutHouse;
		        
		        //Координаты
		        if (Matcher.TryParseCoordinates(deal.Coordinates, out var parsedLatitude, out var parsedLongitude))
			        newDeliveryPoint.SetСoordinates(parsedLatitude, parsedLongitude, uow);
		        else
			        logger.Warn($"Не получилось распарсить координаты {deal.Coordinates}");

		        uow.Save(newDeliveryPoint);
				logger.Info($"Создана новая точка доставки {newDeliveryPoint.CompiledAddress}");
				logger.Info("-------EndProcessDeliveryPoint-------");

		        return newDeliveryPoint;
	        }
        }
		

        private async Task ProcessProducts(IUnitOfWork uow, Deal deal, Order order)
        {
	        var dealProductItems = await bitrixApi.GetProductsForDeal(deal.Id);
	        foreach (var dealProductItem in dealProductItems){
		        Product product = await bitrixApi.GetProduct(dealProductItem.ProductId);
		        bool isOurProduct = IsOurProduct(product);
		        
		        if(isOurProduct) {
			        ProcessOurProduct(uow, deal, order, dealProductItem, product);
		        }
		        else {
			        ProcessOnlineStoreProduct(uow, deal, order, dealProductItem);
		        }
	        }
        }
        
        private void ProcessOurProduct(IUnitOfWork uow, Deal deal, Order order, DealProductItem dealProductItem, Product product)
        {
	        Nomenclature nomenclature = GetNomenclatureForOurProduct(uow, product);
	        if(nomenclature == null) {
		        throw new InvalidOperationException($"Не найдена номенклатура для добавления нашего товара из битрикса. Id номенклатуры в битриксе {product.NomenclatureInfo?.NomenclatureId}");
	        }
	        decimal discount = Math.Abs(nomenclature.GetPrice(1) - dealProductItem.Price);
	        order.AddNomenclature(nomenclature, dealProductItem.Count, discount, true);
        }
        
        private Nomenclature GetNomenclatureForOurProduct(IUnitOfWork uow, Product product)
        {
	        if(product.NomenclatureInfo == null) {
		        throw new InvalidOperationException($"Попытка загрузить номенклатуру для не соответствующего продукта " +
		                                            $"(Для продукта {product.Id} ({product.Name}) не заполнено поле {nameof(product.NomenclatureInfo)})");
	        }
	        
	        Nomenclature nomenclature = uow.GetById<Nomenclature>(product.NomenclatureInfo.NomenclatureId);
	        if(nomenclature == null) {
		        logger.Info($"Для нашего продукта {product.Id} ({product.Name}) не удалось найти номенклатуру по {nameof(product.NomenclatureInfo.NomenclatureId)}");
	        }
	        else {
		        logger.Info($"Для нашего продукта {product.Id} ({product.Name}) найдена номенклатура по {nameof(product.NomenclatureInfo.NomenclatureId)} {nomenclature.Id} ({nomenclature.Name})");
	        }
	        return nomenclature;
        }
        
        private async Task ProcessOnlineStoreProduct(IUnitOfWork uow, Deal deal, Order order, DealProductItem dealProductItem)
        {
	        decimal discount = 0M;
	        bool isDiscountInMoney = false;
	        bool dealHasPromo = !string.IsNullOrEmpty(deal.Promocode);

	        Nomenclature nomenclature = GetNomenclatureForOnlineStoreProduct(uow, dealProductItem);
	        if(nomenclature == null) {
		        nomenclature = await CreateOnlineStoreNomenclature(uow, dealProductItem);
		        nomenclature.UpdatePrice(dealProductItem.Price, dealProductItem.Count);  
	        }
	        else {
		        if(dealHasPromo) {
			        discount = Math.Abs(nomenclature.GetPrice(1) - dealProductItem.Price);
			        isDiscountInMoney = true;
		        }
		        else {
			        nomenclature.UpdatePrice(dealProductItem.Price, dealProductItem.Count);  
		        }
	        }
	        
	        order.AddNomenclature(nomenclature, dealProductItem.Count, discount, isDiscountInMoney);
        }

        private async Task<Nomenclature> CreateOnlineStoreNomenclature(IUnitOfWork uow, DealProductItem dealProductItem)
        {
	        //Если нет такой группы то создаем группу
	        var group = await ProcessProductGroup(uow, dealProductItem);
	        var measurementUnit = measurementUnitsRepository.GetUnitsByBitrix(uow, dealProductItem.MeasureName);
	        if(measurementUnit == null) {
		        throw new InvalidOperationException(
			        $"Не удалось найти единицу измерения в доставке воды для {dealProductItem.MeasureName}");
	        }
	        var nomenclature = new Nomenclature() {
		        Name = dealProductItem.ProductName,
		        OfficialName = dealProductItem.ProductName,
		        Description = dealProductItem.ProductDescription ?? "",
		        CreateDate = DateTime.Now,
		        Category = NomenclatureCategory.additional,
		        BitrixId = dealProductItem.ProductId,
		        VAT = VAT.Vat20,
		        OnlineStoreExternalId = "3",
		        Unit = measurementUnit,
		        ProductGroup = group
	        };
	        return nomenclature;
        }
        
        private Nomenclature GetNomenclatureForOnlineStoreProduct(IUnitOfWork uow, DealProductItem product)
        {
	        Nomenclature nomenclature = null;
	        if (matcher.MatchNomenclatureByBitrixId(uow, product.ProductId, out nomenclature)){
		        logger.Info($"Для продукта ИМ {product.ProductId} ({product.ProductName}) найдена номенклатура по bitrix_id {nomenclature.BitrixId} ({nomenclature.Name})");
	        }
	        else if (matcher.MatchNomenclatureByName(uow, product.ProductName, out nomenclature)){
		        logger.Info($"Для продукта ИМ {product.ProductId} ({product.ProductName}) найдена номенклатура по имени {nomenclature.BitrixId} ({nomenclature.Name})");
	        }

	        if(nomenclature == null) {
		        logger.Info($"Для продукта ИМ {product.ProductId} ({product.ProductName}) не удалось найти соответствующую номенклатуру");
	        }
	        return nomenclature;
        }

        private async Task<ProductGroup> ProcessProductGroup(IUnitOfWork uow, DealProductItem productFromDeal)
        {
	        var product = await bitrixApi.GetProduct(productFromDeal.ProductId);
	        if(product == null) {
		        throw new Exception($"Продукт с id {productFromDeal.ProductId} не найден в битриксе");
	        }
	        
	        var allProductGroups = product.Category.IsOurProduct.Split('/');
	        var lastGroupName = allProductGroups[allProductGroups.Length - 1];
	        
	        if (matcher.MatchNomenclatureGroupByName(uow, lastGroupName, out var productGroup)){
		        return productGroup;
	        }
	        else{
		        var reversedProductGroups = allProductGroups.Take(allProductGroups.Length - 1).Reverse();
		        ProductGroup lastGroupWeHave = null;
		        IList<ProductGroup> allNewProductGroups = new List<ProductGroup>();
		        allNewProductGroups.Add(new ProductGroup {Name = lastGroupName});
		        
		        foreach (var reversedProductGroupName in reversedProductGroups){
			        if (matcher.MatchNomenclatureGroupByName(uow, reversedProductGroupName, out var matchedProductGroup)){
				        allNewProductGroups.Add(matchedProductGroup);
			        }
			        else{
				        var newUnmatchedProductGroup = new ProductGroup()
							{Name = reversedProductGroupName};
				        allNewProductGroups.Add(newUnmatchedProductGroup);
			        }
		        }
		        //matchaed	unmatchad
		        // a / b / (c / d / e) 
		 
		        //Проверяем что a, b связаны правильно
		        //Просто связываем все
		        if (allNewProductGroups.Any()){
			        for (var i = 0; i < allNewProductGroups.Count-1; i++){
				        allNewProductGroups[i].Parent = allNewProductGroups[i + 1];
				        uow.Save(allNewProductGroups[i]);
			        }
		        }
		        
		        //связываем 'c' с 'b'
		        uow.Save(allNewProductGroups.Last());
				return allNewProductGroups.First();
	        }

        }

        private void UpdateOnlineStoreNomenclaturePrice(Nomenclature nomenclature, decimal price)
        {
	        if(!nomenclature.IsOnlineStoreNomenclature) {
		        return;
	        }
	        nomenclature.UpdatePrice(price, 1);
        }

        private bool IsOurProduct(Product product)
        {
	        return product?.NomenclatureInfo?.NomenclatureId > 0;
        }
    }
}