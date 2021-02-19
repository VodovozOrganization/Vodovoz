using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using BitrixApi.DTO;
using BitrixApi.REST;
using QS.DomainModel.UoW;
using QS.Osm.DTO;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Parameters;
using Vodovoz.Repository;
using Vodovoz.Services;
using Contact = BitrixApi.DTO.Contact;
using Phone = Vodovoz.Domain.Contacts.Phone;

namespace BitrixIntegration {
    public class CoR: ICoR {
        private bool needSearchOrder = false;
        private bool needSearchPoint = false;
        private bool needSearchNomenclature = false;
        private bool needSearchCounterParty = false;

        private bool needCreateOrder = false;
        private bool needCreateDeliveryPoint = false;
        private bool needCreateNomenclature = false;
        private bool needCreateCounterParty = false;
        
        private bool isCompany = false;

        private bool needSetFirstOrderForCounterparty = false;

        // private string token;
        private readonly IBitrixRestApi bitrixApi;
        private readonly IUnitOfWork uow;
        private readonly IBitrixServiceSettings bitrixServiceSettings;
        
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Matcher matcher;
        

        public CoR(IBitrixServiceSettings _bitrixServiceSettings, /*string _token,*/ IBitrixRestApi _bitrixRestApi, IUnitOfWork _uow, Matcher _matcher)
        {
	        // token = _token ?? throw new ArgumentNullException(nameof(_token));
	        bitrixApi = _bitrixRestApi ?? throw new ArgumentNullException(nameof(_bitrixRestApi));
	        uow = _uow ?? throw new ArgumentNullException(nameof(_uow));
	        matcher = _matcher ?? throw new ArgumentNullException(nameof(_matcher));
	        bitrixServiceSettings = _bitrixServiceSettings ?? throw new ArgumentNullException(nameof(_bitrixServiceSettings));
	        QS.Osm.Osrm.OsrmMain.ServerUrl = "http://osrm.vod.qsolution.ru:5000";
        }

        public async Task Process(uint dealId)
        {
	        try{
		        needSetFirstOrderForCounterparty = false;
		        needCreateNomenclature = false;
		        
		        var deal = await ProcessDeal(dealId);
		        if (deal == null){
			        logger.Error("Ошибка при получении сделки из BitrixApi");
			        return;
		        }

		        logger.Info($"Обработка Deal: {deal.Id}");
		        bool isSelfDelivery = deal.DeliveryType == "626";
		        bool isSmsPayment = deal.PaymentMethod == "1108";
		        if (isSelfDelivery)
			        logger.Info("Сделка является самовызовом");
		        if (isSmsPayment)
			        logger.Info("Сделка оплачивается по СМС");
		        

		        //ищем у нас сделку по битрикс Id
		        needCreateOrder = !matcher.MatchOrderByBitrixId(uow, deal.Id, out var ourOrder);
		        if (ourOrder != null){
			        logger.Info($"Сделка {deal.Id} найдена у нас под id: {ourOrder.Id}, обработка не требуется");
			        return;
		        } else if (deal.CreateInDV == 0)
		        {
			        logger.Info($"Сделка {deal.Id} имеет статус отличный от Завести в ДВ");
			        // return;
		        }
		        
		        logger.Info("Обработка контрагента");
		        Counterparty counterpartyForNewOrder =
			        await ProcessCounterparty(
				        deal); //TODO gavr потом добавить к ней адрес, когда он станет известен
		        
		        DeliverySchedule deliveryScheduleForOrder = null;
		        DeliveryPoint deliveryPointForOrder = null;
		        
		        if (!isSelfDelivery){
			        logger.Info("Обработка точек доставки");
			        needCreateNomenclature = false;
			        needSearchNomenclature = false;
			        deliveryPointForOrder = ProcessDeliveryPoint(deal, counterpartyForNewOrder);
			        deliveryScheduleForOrder = DeliveryScheduleRepository.GetByBitrixId(uow, deal.DeliverySchedule);
		        }

		        //точку доставки не ищем, её можно только начать сопоставлять тк кк она в сделке
		        //TODO удалить bitrixId у точек доставки из таблицы, сущности и маппинга 
		        logger.Info("Сборка заказа");
		        var newOrder = ProcessOrder(
			        deal,
			        deliveryPointForOrder,
			        counterpartyForNewOrder, 
			        isSmsPayment,
			        deliveryScheduleForOrder
			    );
		        
		        logger.Info("Обработка номенклатур");
		        var orderWithNomenclatures = await ProcessNomenclaturesAndAddToOrder(deal, newOrder);

		        uow.Save(orderWithNomenclatures);
		        uow.Commit();
	        }
			catch (Exception e){
				logger.Error($"При обработке заказа с id {dealId} возникла ошибка: {e.Message}\n");
				logger.Error($"Внутренняя ошибка: {e.InnerException}");
			}
			
        }

        private Order ProcessOrder(
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
		        Trifle = 0,
		        BottlesReturn = deal.BottlsToReturn,
		        Contract = new CounterpartyContract()
		        // Onli
	        };

	        if (needSetFirstOrderForCounterparty){
		        counterpartyForNewOrder.FirstOrder = newOrder;
	        }
	       
	        logger.Info("-------------------------");
	        return newOrder;
        }


        async Task<Deal> ProcessDeal(uint id)
        {
	        //Получаем сделку из битрикса
	        var deal = await bitrixApi.GetDealAsync(id);
	        if (deal == null){
		        throw new NoNullAllowedException($"Сделка с Id:{id} не найдена в Bitrix24 запросом crm.deal.get");
	        }
	        //Определение это там контакт или компания
	        if (deal.CompanyId != 0)
		        isCompany = true;
	        else
		        isCompany = false;

	        return deal;
        }
        async Task<Counterparty> ProcessCounterparty(Deal deal)
        {
	        if (deal.CompanyId == 0){
		        return await ProcessContact(deal);
	        } else {
		        return await ProcessCompany(deal);
	        }
        }

        async Task<Counterparty> ProcessContact(Deal deal)
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
		        AddPhonesToCounterpartyFromContact(newCounterparty, contact);
		        
		        uow.Save(newCounterparty);
		        logger.Info("-------EndProcessContact-------");

		        return newCounterparty;
	        }
        }

        async Task<Counterparty> ProcessCompany(Deal deal)
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
		        AddPhonesToCounterpartyFromCompany(newCounerparty, company);
		        
		        uow.Save(newCounerparty);
		        logger.Info("-------EndProcessCompany-------");

		        return newCounerparty;
	        }
        }

        void AddPhonesToCounterpartyFromContact(Counterparty newCounterparty, Contact contact)
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
        
        void AddPhonesToCounterpartyFromCompany(Counterparty newCounterparty, Company company)
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

        DeliveryPoint ProcessDeliveryPoint(Deal deal, Counterparty counterpartyForNewOrder)
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

        async Task<Order> ProcessNomenclaturesAndAddToOrder(Deal deal, Order newOrder)
        {
			//TODO для ускорения не матчить по одинаковые
	        bool isOurProduct = false; // TODO gavr определять это из товара когда это там появиться 
	        var productList = await bitrixApi.GetProductsForDeal(deal.Id);
	        //ищем у нас товары по битрикс Id
	        // IList<Nomenclature> matchedNomenclatures = new List<Nomenclature>();
	        IList<ProductFromDeal> unmatchedProducts = new List<ProductFromDeal>();
			
	        foreach (var productBitrix in productList){
		        if (matcher.MatchNomenclatureByBitrixId(uow, productBitrix.Id, out var ourNomenclature)){
			        logger.Info($"Номенклатура {productBitrix.ProductName} сопоставилась по id {ourNomenclature.BitrixId}");
			        UpdateNomenclaturePriceIfNeededAndAdd(deal, newOrder, productBitrix, ourNomenclature, false);
		        }
		        else if (matcher.MatchNomenclatureByName(uow, productBitrix.ProductName, out var outNomenclature)){
			        logger.Info($"Номенклатура {productBitrix.ProductName} сопоставилась имени");
			        newOrder.AddNomenclature(outNomenclature, productBitrix.Count);
		        }
		        else{
			        unmatchedProducts.Add(productBitrix);
		        }
	        }

	        if (unmatchedProducts.Count != 0){
		        needCreateNomenclature = true;
		        // using (var uowForNewNomenclatures = UnitOfWorkFactory.CreateWithNewRoot<Nomenclature>()){
			       //  uowForNewNomenclatures.Root. = deal.GetRoomType();
		        //
		        //
			       //  uowForNewNomenclatures.Save();
			       //  uowForNewNomenclatures.Commit();
			       //  
			       //  counterpartyForNewOrder.DeliveryPoints.Add(uowForNewNomenclatures.Root);
		        //  return uowForNewNomenclatures.Root;
					logger.Error($"Есть несовпавшие номенклатуры");

			       throw new NotImplementedException();
			       // }
	        }
	        return newOrder;
        }
        
        /*
	        *	1) если UF_CRM_1596187803 null то мы обновляем цену товара в ДВ на ту что пришла из сделки
			   2) если UF_CRM_1596187803 не null и товар сопоставился по названию или bitrix id, то отнимаем от цены ДВ цену 
				  которая пришла и проставляем это значение как скидку в рублях
				  Второй пункт переносим в обработку заказа потому что цены ордер итемов там
			   3) Если товара нет в базе, то создаем его заполняя данными битрикса
	        */
        private void UpdateNomenclaturePriceIfNeededAndAdd(
	        Deal deal, 
	        Order newOrder,
	        ProductFromDeal productBitrix,
	        Nomenclature nomenclature,
	        bool isOurNomenclature
	        )
        {
	        bool dealHasPromo = !string.IsNullOrEmpty(deal.Promocode);
	        if (productBitrix.Price != nomenclature.GetPrice(1)){
		        if (string.IsNullOrWhiteSpace(deal.Promocode)){
			        nomenclature.SetPrice(productBitrix.Price, 1);
			        uow.Save(nomenclature);
			        newOrder.AddNomenclature(nomenclature, productBitrix.Count);
		        } else if (dealHasPromo && !needCreateNomenclature){
			        var discount = Math.Abs(nomenclature.GetPrice(1) - productBitrix.Price);
				    newOrder.AddNomenclature(nomenclature, productBitrix.Count, discount, true);
				    uow.Save(newOrder);
		        }
	        }
        }

        private decimal GetMoneyDiscountIfNeeded(IUnitOfWork uow, Deal deal, ProductFromDeal productBitrix, Nomenclature ourNomenclature, bool isOurNomenclature, bool orderMatched)
        {
	        

	        throw new NotImplementedException();
        }
    }
}