using Bitrix.DTO;
using Bitrix;
using NLog;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Common;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Factories;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;
using VodovozInfrastructure.Utils;
using BitrixPhone = Bitrix.DTO.Phone;
using Phone = Vodovoz.Domain.Contacts.Phone;
using Contact = Bitrix.DTO.Contact;
using Vodovoz.Domain.Common;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.BasicHandbooks;

namespace BitrixIntegration
{
	//Класс перегружен, его необходимо разделить на зависимости отвечающие за свои действия
	public class DealProcessor
    {
	    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IBitrixClient _bitrixClient;
        private readonly ICounterpartyContractRepository _counterpartyContractRepository;
        private readonly CounterpartyContractFactory _counterpartyContractFactory;
		private readonly DealRegistrator _dealRegistrator;
		private readonly IMeasurementUnitsRepository _measurementUnitsRepository;
        private readonly IBitrixServiceSettings _bitrixServiceSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;

		public DealProcessor(
			IUnitOfWorkFactory uowFactory,
			IBitrixClient bitrixClient, 
	        ICounterpartyContractRepository counterpartyContractRepository,
	        CounterpartyContractFactory counterpartyContractFactory,
			DealRegistrator dealRegistrator,
	        IMeasurementUnitsRepository measurementUnitsRepository,
	        IBitrixServiceSettings bitrixServiceSettings,
			IOrderRepository orderRepository,
			ICounterpartyRepository counterpartyRepository,
			INomenclatureRepository nomenclatureRepository,
			IDeliveryScheduleRepository deliveryScheduleRepository
			)
        {
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_bitrixClient = bitrixClient ?? throw new ArgumentNullException(nameof(bitrixClient));
	        _bitrixServiceSettings = bitrixServiceSettings ?? throw new ArgumentNullException(nameof(bitrixServiceSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
			_counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
	        _counterpartyContractFactory = counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			_dealRegistrator = dealRegistrator ?? throw new ArgumentNullException(nameof(dealRegistrator));
			_measurementUnitsRepository = measurementUnitsRepository ?? throw new ArgumentNullException(nameof(measurementUnitsRepository));
        }

		public void ProcessDeals(DateTime date)
		{
			var startDay = date.Date;
			var endDay = date.Date.AddDays(1).AddMilliseconds(-1);

			var deals = _bitrixClient.GetDeals(startDay, endDay);
			foreach(var deal in deals)
			{
				_dealRegistrator.RegisterDealAsInProgress(deal.Id);
				ProcessDeal(deal);
			}
		}

        private void ProcessDeal(Deal deal)
        {
	        _logger.Info($"Обработка сделки: {deal.Id}");

			//ВНИМАНИЕ! Тут реализована обработка сделки на основании создания нового заказа
			//необходимо сделать обработку заказа универсально как для нового так и для существующего
			//чтобы без опаски можно было прогнать сделку на существующем заказе и актуализировать информацию в нем

	        using(var uow = _uowFactory.CreateWithoutRoot()) {
				var order = FindExistingOrder(uow, deal.Id);
				if(order != null)
				{
					_logger.Info($"Обработка сделки пропущена. Для сделки №{deal.Id} уже есть существующий заказ №{order.Id}.");
					return;
				}

		        _logger.Info("Обработка контрагента");
		        Counterparty counterparty = GetCounterpartyFromDeal(uow, deal);

		        _logger.Info("Сборка заказа");
		        order = CreateOrder(uow, deal, counterparty);

		        _logger.Info("Обработка номенклатур");
		        ProcessProducts(uow, deal, order);

		        foreach(var orderItem in order.OrderItems) {
			        uow.Save(orderItem.Nomenclature);
		        }

		        uow.Save(order);
		        uow.Commit();
	        }
        }

		private Order FindExistingOrder(IUnitOfWork uow, uint dealId)
		{
			var order = _orderRepository.GetOrderByBitrixId(uow, dealId);
			if(order == null)
			{
				//Тут необходимо реализовать поиск заказа по контрагенту и точке доставке на день доставки
			}

			return order;
		}

        private Order CreateOrder(IUnitOfWork uow, Deal deal, Counterparty counterparty)
        {
			DeliveryPoint deliveryPoint = null;
			DeliverySchedule deliverySchedule = null;

			if(!deal.IsSelfDelivery)
			{
				_logger.Info("Обработка точек доставки");
				deliveryPoint = ProcessDeliveryPoint(uow, deal, counterparty);
				deliverySchedule = _deliveryScheduleRepository.GetByBitrixId(uow, deal.DeliverySchedule);
				if(deliverySchedule == null)
				{
					throw new InvalidOperationException($"Не найдено время доставки DeliverySchedule ({deal.DeliverySchedule}) по bitrixId");
				}
			}

			var bitrixAccount = uow.GetById<Employee>(_bitrixServiceSettings.EmployeeForOrderCreate);
	        var order = new Order()
	        {
		        UoW = uow,
		        BitrixDealId = deal.Id,
		        PaymentType = deal.GetPaymentMethod(),
		        CreateDate = deal.CreateDate,
		        DeliveryDate = deal.DeliveryDate,
		        DeliverySchedule = deliverySchedule,
		        Client = counterparty,
		        DeliveryPoint = deliveryPoint,
		        OrderStatus = OrderStatus.Accepted,
		        Author = bitrixAccount,
		        LastEditor = bitrixAccount,
		        LastEditedTime = DateTime.Now,
		        PaymentBySms = deal.IsSmsPayment,
		        OrderPaymentStatus = deal.GetOrderPaymentStatus(),
		        SelfDelivery = deal.IsSelfDelivery,
		        Comment = deal.Comment,
		        Trifle = deal.Trifle ?? 0,
		        BottlesReturn = deal.BottlesToReturn,
		        EShopOrder = (int)deal.Id,
		        OnlineOrder = deal.OrderNumber ?? null
	        };

	        if (order.PaymentType == PaymentType.ByCard)
	        {
		        order.PaymentByCardFrom = uow.GetById<PaymentFrom>(7);
	        }

			order.UpdateOrCreateContract(uow, _counterpartyContractRepository, _counterpartyContractFactory);

	        return order;
        }

        private Counterparty GetCounterpartyFromDeal(IUnitOfWork uow, Deal deal)
        {
	        if(deal.ContactId != 0){
				return GetContact(uow, deal);
	        }

	        if(deal.CompanyId != 0){
				return GetCompany(uow, deal);
	        }

			throw new InvalidOperationException("Сделка не имеет ни контакта ни компании, такие сделки невозможно обработать");
        }

        private Counterparty GetContact(IUnitOfWork uow, Deal deal)
        {
	        _logger.Info("Обработка контрагента как контакта");

	        var contact = _bitrixClient.GetContact(deal.ContactId);
			if(contact == null)
			{
				throw new InvalidOperationException($"Не удалось загрузить контакт №{deal.ContactId}");
			}

			Counterparty counterparty = GetCounterpartyOrNull(uow, contact);
			if(counterparty == null)
			{
				_logger.Info($"Не найден контрагент для контакта: {contact.Id} {contact.SecondName} {contact.Name} {contact.LastName} " +
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
			}

			return counterparty;
		}

        private Counterparty GetCompany(IUnitOfWork uow, Deal deal)
        {
			_logger.Info("Обработка контрагента как компании");

			var company = _bitrixClient.GetCompany(deal.CompanyId);
			if(company == null)
			{
				throw new InvalidOperationException($"Не удалось загрузить компанию №{deal.CompanyId}");
			}

			Counterparty counterparty = GetCounterpartyOrNull(uow, company);
			if(counterparty == null)
			{
				_logger.Info($"Не найден контрагент для компании: {company.Title}, создаем нового контрагента");
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
	        }

		    return counterparty;
		}

		private Counterparty GetCounterpartyOrNull(IUnitOfWork uow, Contact contact)
		{
			Counterparty counterparty = _counterpartyRepository.GetCounterpartyByBitrixId(uow, contact.Id);
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

			IList<Counterparty> counterparties = _counterpartyRepository.GetCounterpartiesByNameAndPhone(uow, contactName, digitsNumber);
			var count = counterparties.Count;
			if(count == 1)
			{
				counterparty = counterparties.First();
				_logger.Info($"Для контакта с BitrixId {contact.Id} у нас найден 1 контрагент {counterparty.Id} по телефону и части имени");
				return counterparty;
			}
			else if(count > 1)
			{
				var ids = counterparties.Select(x => x.Id);
				var counterpartyIds = string.Join(", ", ids);
				_logger.Info($"Для контакта с BitrixId {contact.Id} найдено несколько контрагентов ({counterpartyIds}) " +
					$"по телефону и части имени. Невозможно выбрать кого-то одного");
				return null;
			}
			else
			{
				_logger.Info($"Для контакта с BitrixId {contact.Id} не найдено контрагентов по телефону и части имени");
				return null;
			}
		}

		private Counterparty GetCounterpartyOrNull(IUnitOfWork uow, Company company)
		{
			Counterparty counterparty = _counterpartyRepository.GetCounterpartyByBitrixId(uow, company.Id);
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

			IList<Counterparty> counterparties = _counterpartyRepository.GetCounterpartiesByNameAndPhone(uow, company.Title, digitsNumber);
			var count = counterparties.Count;
			if(count == 1)
			{
				counterparty = counterparties.First();
				_logger.Info($"Для компании с BitrixId {company.Id} у нас найден 1 контрагент {counterparty.Id} по телефону и названию");
				return counterparty;
			}
			else if(count > 1)
			{
				var ids = counterparties.Select(x => x.Id);
				var counterpartyIds = string.Join(", ", ids);
				_logger.Info($"Для компании с BitrixId {company.Id} найдено несколько контрагентов ({counterpartyIds}) " +
					$"по телефону и названию. Невозможно выбрать кого-то одного");
				return null;
			}
			else
			{
				_logger.Info($"Для компании с BitrixId {company.Id} не найдено контрагентов по телефону и названию");
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
		        _logger.Info($"Добавляем телефон: {contactPhone} контрагенту {counterparty.FullName}");
				counterparty.Phones.Add(phone);
	        }
        }

        private DeliveryPoint ProcessDeliveryPoint(IUnitOfWork uow, Deal deal, Counterparty counterparty)
        {
			//ЗАГЛУШКА, ПОКА НЕ БУДЕТ РЕАЛИЗОВАН ТОЧНЫЙ АДРЕСС В БИТРИКС
			return counterparty.DeliveryPoints.FirstOrDefault();

			//Парсинг координат
			Coordinate coordinate = Coordinate.Parse(deal.Coordinates);
        }

		private void ProcessProducts(IUnitOfWork uow, Deal deal, Order order)
        {
	        var dealProductItems = _bitrixClient.GetProductsForDeal(deal.Id);
	        foreach (var dealProductItem in dealProductItems){
		        Product product = _bitrixClient.GetProduct(dealProductItem.ProductId);
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
		        _logger.Info($"Для нашего продукта {product.Id} ({product.Name}) не удалось найти номенклатуру по {nameof(product.NomenclatureInfo.NomenclatureId)}");
	        }
	        else {
		        _logger.Info($"Для нашего продукта {product.Id} ({product.Name}) найдена номенклатура по {nameof(product.NomenclatureInfo.NomenclatureId)} {nomenclature.Id} ({nomenclature.Name})");
	        }
	        return nomenclature;
        }
        
        private void ProcessOnlineStoreProduct(IUnitOfWork uow, Deal deal, Order order, DealProductItem dealProductItem)
        {
	        decimal discount = 0M;
	        bool isDiscountInMoney = false;
	        bool dealHasPromo = !string.IsNullOrEmpty(deal.Promocode);

	        Nomenclature nomenclature = GetNomenclatureForOnlineStoreProduct(uow, dealProductItem);
	        if(nomenclature == null) {
		        nomenclature = CreateOnlineStoreNomenclature(uow, dealProductItem);
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

        private Nomenclature CreateOnlineStoreNomenclature(IUnitOfWork uow, DealProductItem dealProductItem)
        {
	        //Если нет такой группы то создаем группу
	        var group = ProcessProductGroup(uow, dealProductItem);
	        var measurementUnit = _measurementUnitsRepository.GetUnitsByBitrix(uow, dealProductItem.MeasureName);
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
	        if (MatchNomenclatureByBitrixId(uow, product.ProductId, out nomenclature)){
		        _logger.Info($"Для продукта ИМ {product.ProductId} ({product.ProductName}) найдена номенклатура по bitrix_id {nomenclature.BitrixId} ({nomenclature.Name})");
	        }
	        else if (MatchNomenclatureByName(uow, product.ProductName, out nomenclature)){
		        _logger.Info($"Для продукта ИМ {product.ProductId} ({product.ProductName}) найдена номенклатура по имени {nomenclature.BitrixId} ({nomenclature.Name})");
	        }

	        if(nomenclature == null) {
		        _logger.Info($"Для продукта ИМ {product.ProductId} ({product.ProductName}) не удалось найти соответствующую номенклатуру");
	        }
	        return nomenclature;
        }

        private ProductGroup ProcessProductGroup(IUnitOfWork uow, DealProductItem productFromDeal)
        {
	        var product = _bitrixClient.GetProduct(productFromDeal.ProductId);
	        if(product == null) {
		        throw new Exception($"Продукт с id {productFromDeal.ProductId} не найден в битриксе");
	        }
	        
	        var allProductGroups = product.Category.IsOurProduct.Split('/');
	        var lastGroupName = allProductGroups[allProductGroups.Length - 1];
	        
	        if (MatchNomenclatureGroupByName(uow, lastGroupName, out var productGroup)){
		        return productGroup;
	        }
	        else{
		        var reversedProductGroups = allProductGroups.Take(allProductGroups.Length - 1).Reverse();

		        IList<ProductGroup> allNewProductGroups = new List<ProductGroup>();
		        allNewProductGroups.Add(new ProductGroup {Name = lastGroupName});
		        
		        foreach (var reversedProductGroupName in reversedProductGroups){
			        if (MatchNomenclatureGroupByName(uow, reversedProductGroupName, out var matchedProductGroup)){
				        allNewProductGroups.Add(matchedProductGroup);
			        }
			        else{
				        var newUnmatchedProductGroup = new ProductGroup()
							{Name = reversedProductGroupName};
				        allNewProductGroups.Add(newUnmatchedProductGroup);
			        }
		        }

		        if (allNewProductGroups.Any()){
			        for (var i = 0; i < allNewProductGroups.Count-1; i++){
				        allNewProductGroups[i].Parent = allNewProductGroups[i + 1];
				        uow.Save(allNewProductGroups[i]);
			        }
		        }
		        
		        uow.Save(allNewProductGroups.Last());
				return allNewProductGroups.First();
	        }
        }

        private bool IsOurProduct(Product product)
        {
	        return product?.NomenclatureInfo?.NomenclatureId > 0;
        }

		public bool MatchNomenclatureByBitrixId(IUnitOfWork uow, uint productId, out Nomenclature outNomenclature)
		{
			Nomenclature nomenclature = null;
			nomenclature = _nomenclatureRepository.GetNommenclatureByBitrixId(uow, productId);

			if(nomenclature == null)
			{
				outNomenclature = null;
				_logger.Info($"Не удалось сопоставить Nommenclature по BitrixId: {productId}");

				return false;
			}
			else
			{
				outNomenclature = nomenclature;
				_logger.Info($"Сопоставление Counterparty: {outNomenclature.Id} по BitrixId: {productId} прошло успешно");
				return true;
			}
		}

		public bool MatchNomenclatureByName(IUnitOfWork uow, string productName, out Nomenclature outNomenclature)
		{
			outNomenclature = _nomenclatureRepository.GetNomenclatureByName(uow, productName);
			if(outNomenclature == null)
			{
				_logger.Warn($"Номенклатура не найдена по названию {productName}");
				return false;
			}
			return true;
		}

		public bool MatchNomenclatureGroupByName(IUnitOfWork uow, string lastGroup, out ProductGroup outProductGroup)
		{
			var group = _nomenclatureRepository.GetProductGroupByName(uow, lastGroup);
			if(group != null)
			{
				outProductGroup = group;
				return true;
			}
			else
			{
				outProductGroup = null;
				return false;
			}
		}
	}
}
