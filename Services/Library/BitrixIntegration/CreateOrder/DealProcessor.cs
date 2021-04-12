using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BitrixApi.DTO;
using BitrixApi.REST;
using NLog;
using QS.DomainModel.UoW;
using QS.Osm.DTO;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Common;
using Vodovoz.Parameters;
using Vodovoz.Repositories.Client;
using Vodovoz.Repository;
using Vodovoz.Services;
using Contact = BitrixApi.DTO.Contact;
using Phone = Vodovoz.Domain.Contacts.Phone;

namespace BitrixIntegration {
    public class DealProcessor: ICoR 
    {
	    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private bool needSetFirstOrderForCounterparty = false;

        private readonly IBitrixRestApi bitrixApi;
        private readonly Matcher matcher;
        private readonly ICounterpartyContractRepository counterpartyContractRepository;
        private readonly CounterpartyContractFactory counterpartyContractFactory;
        private readonly IMeasurementUnitsRepository measurementUnitsRepository;
        private readonly IBitrixServiceSettings bitrixServiceSettings;

        public DealProcessor(
	        IBitrixRestApi bitrixApi, 
	        Matcher matcher,
	        ICounterpartyContractRepository counterpartyContractRepository,
	        CounterpartyContractFactory counterpartyContractFactory,
	        IMeasurementUnitsRepository measurementUnitsRepository,
	        IBitrixServiceSettings bitrixServiceSettings
	        )
        {
	        this.bitrixApi = bitrixApi ?? throw new ArgumentNullException(nameof(bitrixApi));
	        this.matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
	        this.bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
	        this.counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
	        this.counterpartyContractFactory = counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
	        this.measurementUnitsRepository = measurementUnitsRepository ?? throw new ArgumentNullException(nameof(measurementUnitsRepository));
	        QS.Osm.Osrm.OsrmMain.ServerUrl = bitrixServiceSettings.OsrmServiceURL;
        }

        public async Task<Order> Process(Deal deal)
        {
	        needSetFirstOrderForCounterparty = false;

	        logger.Info($"Обработка Deal: {deal.Id}");
	        //TODO Bitrix Проверить коды, возможно лучше вынести их куда-то в константы или параметры
	        bool isSelfDelivery = deal.DeliveryType == "626";
	        bool isSmsPayment = deal.PaymentMethod == "1108";
	        
	        if (isSelfDelivery)
		        logger.Info("Сделка является самовызовом");
	        if (isSmsPayment)
		        logger.Info("Сделка оплачивается по СМС");

	        using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {	
		        matcher.MatchOrderByBitrixId(uow, deal.Id, out var ourOrder);
		        if (ourOrder != null) {
			        logger.Info($"Сделка {deal.Id} найдена у нас под id: {ourOrder.Id}, обработка не требуется");
			        return ourOrder; 
		        }
		        else if (deal.CreateInDV == 0) {
			        logger.Warn($"Сделка {deal.Id} имеет статус отличный от Завести в ДВ");
		        }

		        logger.Info("Обработка контрагента");
		        Counterparty counterpartyForNewOrder = await ProcessCounterparty(uow, deal); 

		        DeliverySchedule deliveryScheduleForOrder = null;
		        DeliveryPoint deliveryPointForOrder = null;

		        if(!isSelfDelivery) {
			        logger.Info("Обработка точек доставки");
			        deliveryPointForOrder = ProcessDeliveryPoint(uow, deal, counterpartyForNewOrder);
			        deliveryScheduleForOrder = DeliveryScheduleRepository.GetByBitrixId(uow, deal.DeliverySchedule);
			        if(deliveryScheduleForOrder == null) {
				        throw new Exception("Не найдено время в DeliveryShedule по bitrixId");
			        }
		        }

		        logger.Info("Сборка заказа");
		        var order = ProcessOrder(
			        uow,
			        deal,
			        deliveryPointForOrder,
			        counterpartyForNewOrder,
			        isSmsPayment,
			        deliveryScheduleForOrder
		        );

		        logger.Info("Обработка номенклатур");
		        await ProcessProducts(uow, deal, order);

		        foreach(var orderItem in order.OrderItems) {
			        uow.Save(orderItem.Nomenclature);
		        }

		        uow.Save(order);
		        uow.Commit();
		        return order;
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
		        BitrixId = deal.Id,
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
		        BottlesReturn = deal.BottlsToReturn,
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


        private async Task<Deal> GetDeal(uint id)
        {
	        //Получаем сделку из битрикса
	        var deal = await bitrixApi.GetDealAsync(id);
	        if(deal == null) {
		        throw new InvalidOperationException($"Сделка с Id:{id} не найдена в Bitrix24 запросом crm.deal.get");
	        }
	        return deal;
        }

        private async Task<Counterparty> ProcessCounterparty(IUnitOfWork uow, Deal deal)
        {
	        if(deal.CompanyId == 0) {
		        return await ProcessContact(uow, deal);
	        }
	        else {
		        return await ProcessCompany(uow, deal);
	        }
        }

        private async Task<Counterparty> ProcessContact(IUnitOfWork uow, Deal deal)
        {
	        logger.Info("-------ProcessContact-------");
	        //Получаем клиента из сделки
	        var contact = await bitrixApi.GetContact(deal.ContancId);
	        
	        //ищем у нас контакт по битрикс Id
	        if (matcher.MatchCounterpartyByBitrixId(uow, contact.Id, out var matchedByBitrixId)){
		        return matchedByBitrixId;
	        }
	        else if (matcher.MatchCounterpartyByPhoneAndSecondName(uow, contact, out var matchedByPhoneSecondName)){
		        return matchedByPhoneSecondName;
	        }
	        else{
		        logger.Info($"Контакт: {contact.Id} {contact.SecondName} {contact.Name} {contact.LastName} " +
		                    "не сопоставился, создаем новую сущность");
		        needSetFirstOrderForCounterparty = true;
		        var newCounterparty = new Counterparty()
		        {
			        FullName = contact.SecondName + " " + contact.Name + " " + contact.LastName,
			        Name = contact.SecondName + " " + contact.Name + " " + contact.LastName,
			        BitrixId = contact.Id,
			        PersonType = PersonType.natural,
			        CreateDate = contact.CreatedDate,
			        PaymentMethod = deal.GetPaymentMethod(),
			        CounterpartyContracts = new List<CounterpartyContract>()
		        };
		        AddPhonesToCounterpartyFromContact(uow, newCounterparty, contact);
		        
		        uow.Save(newCounterparty);
		        logger.Info("-------EndProcessContact-------");

		        return newCounterparty;
	        }
        }

        private async Task<Counterparty> ProcessCompany(IUnitOfWork uow, Deal deal)
        {
	        logger.Info("-------ProcessCompany-------");

	        //Получаем клиента из сделки
	        var company = await bitrixApi.GetCompany(deal.CompanyId);
	        
	        //ищем у нас контакт или компанию по битрикс Id
	        if (matcher.MatchCounterpartyByBitrixId(uow, company.Id, out var matchedByBitrixId)){
		        return matchedByBitrixId;
	        }
	        else if (matcher.MatchCompanyPhoneAndName(uow, company, out var matchedByPhoneSecondName)){
		        return matchedByPhoneSecondName;
	        }
	        else{
		        logger.Info($"Компания: {company.Id} {company.Title} не сопоставилась, создаем новую сущность");
		        needSetFirstOrderForCounterparty = true;
		        var newCounerparty = new Counterparty()
		        {
			        FullName = company.Title,
			        Name = company.Title,
			        BitrixId = company.Id,
			        PersonType =  PersonType.legal,
			        TypeOfOwnership = VodovozInfrastructure.Utils.NamesUtils.TryGetOrganizationType (company.Title) ?? "",
			        CreateDate = company.DateCreate,
			        PaymentMethod = deal.GetPaymentMethod(),
			        IsArchive = false,
			        CounterpartyContracts = new List<CounterpartyContract>()
		        };
		        AddPhonesToCounterpartyFromCompany(uow, newCounerparty, company);
		        
		        uow.Save(newCounerparty);
		        logger.Info("-------EndProcessCompany-------");

		        return newCounerparty;
	        }
        }

        private void AddPhonesToCounterpartyFromContact(IUnitOfWork uow, Counterparty newCounterparty, Contact contact)
        {
	        //Конвертация телефонов из дто в нормальные
	        IList<Phone> phonesForNewCounterparty = new List<Phone>();

	        foreach (var phone in contact.Phones){
		        var newPhone = new Phone();
		        newPhone.Init(ContactParametersProvider.Instance);
		        newPhone.Number = phone.Value;
		        phonesForNewCounterparty.Add(newPhone);
		        logger.Info($"Добавляем телефон: {phone} контакту {newCounterparty.FullName}");
		        uow.Save(newPhone);
	        }
	        newCounterparty.Phones = phonesForNewCounterparty;
	        uow.Save(newCounterparty);
        }
        
        private void AddPhonesToCounterpartyFromCompany(IUnitOfWork uow, Counterparty newCounterparty, Company company)
        {
	        //Конвертация телефонов из дто в нормальные
	        IList<Phone> phonesForNewCounterparty = new List<Phone>();

	        foreach (var phone in company.Phones){
		        var newPhone = new Phone();
		        newPhone.Init(ContactParametersProvider.Instance);
		        newPhone.Number = phone.Value;
		        logger.Info($"Добавляем телефон: {phone} компании {newCounterparty.FullName}");
		        phonesForNewCounterparty.Add(newPhone);
		        uow.Save(newPhone);
	        }
	        newCounterparty.Phones = phonesForNewCounterparty;
	        uow.Save(newCounterparty);
        }

        private DeliveryPoint ProcessDeliveryPoint(IUnitOfWork uow, Deal deal, Counterparty counterpartyForNewOrder)
        {
	        logger.Info("-------ProcessDeliveryPoint-------");
	        if (counterpartyForNewOrder == null) 
		        throw new ArgumentNullException(nameof(counterpartyForNewOrder));
	        
	        logger.Info($"Сопоставление точки доставки: {deal.DeliveryAddressWithoutHouse} " +
	                    $"для клиента {counterpartyForNewOrder?.FullName}");
	        
	        if (matcher.MatchDeliveryPoint(uow, deal, counterpartyForNewOrder, out var deliveryPoint) ){
		        
		        //Если к нам пришел незаполненный адресами контрпати, то привязываем ему адрес
		        if (counterpartyForNewOrder?.DeliveryPoints.Count == 0){
			        counterpartyForNewOrder.DeliveryPoints.Add(deliveryPoint);
		        }
		        logger.Info("-------EndProcessDeliveryPoint-------");
		        return deliveryPoint;
	        }
	        else{
		        logger.Info("Создание новой сущности DeliveryPoint");

		        var newDeliveryPoint = DeliveryPoint.Create(counterpartyForNewOrder);
		        
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
        
        private void ProcessOurProduct(IUnitOfWork uow, Deal deal, Order order, ProductFromDeal dealProductItem, Product product)
        {
	        Nomenclature nomenclature = GetNomenclatureForOurProduct(uow, product);
	        if(nomenclature == null) {
		        throw new InvalidOperationException($"Не найдена номенклатура для добавления нашего товара из битрикса. Id номенклатуры в битриксе {product.ErpNomenclatureId}");
	        }
	        decimal discount = Math.Abs(nomenclature.GetPrice(1) - dealProductItem.Price);
	        order.AddNomenclature(nomenclature, dealProductItem.Count, discount, true);
        }
        
        private Nomenclature GetNomenclatureForOurProduct(IUnitOfWork uow, Product product)
        {
	        if(product.ErpNomenclatureId == 0) {
		        throw new InvalidOperationException($"Попытка загрузить номенклатуру для не соответствующего продукта " +
		                                            $"(Для продукта {product.Id} ({product.Name}) не заполнено поле {nameof(product.ErpNomenclatureId)})");
	        }
	        
	        Nomenclature nomenclature = uow.GetById<Nomenclature>(product.ErpNomenclatureId);
	        if(nomenclature == null) {
		        logger.Info($"Для нашего продукта {product.Id} ({product.Name}) не удалось найти номенклатуру по {nameof(product.ErpNomenclatureId)}");
	        }
	        else {
		        logger.Info($"Для нашего продукта {product.Id} ({product.Name}) найдена номенклатура по {nameof(product.ErpNomenclatureId)} {nomenclature.Id} ({nomenclature.Name})");
	        }
	        return nomenclature;
        }
        
        private async Task ProcessOnlineStoreProduct(IUnitOfWork uow, Deal deal, Order order, ProductFromDeal dealProductItem)
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

        private async Task<Nomenclature> CreateOnlineStoreNomenclature(IUnitOfWork uow, ProductFromDeal dealProductItem)
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
        
        private Nomenclature GetNomenclatureForOnlineStoreProduct(IUnitOfWork uow, ProductFromDeal product)
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

        private async Task<ProductGroup> ProcessProductGroup(IUnitOfWork uow, ProductFromDeal productFromDeal)
        {
	        var product = await bitrixApi.GetProduct(productFromDeal.ProductId);
	        if(product == null) {
		        throw new Exception($"Продукт с id {productFromDeal.ProductId} не найден в битриксе");
	        }
	        
	        var allProductGroups = product.CategoryObj.IsOurProduct.Split('/');
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
	        return product?.ErpNomenclatureId > 0;
        }
    }
}