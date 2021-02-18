using System;
using System.Collections;
using System.Collections.Generic;
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

        private string token;
        private readonly IBitrixRestApi bitrixApi;
        private readonly IUnitOfWork uow;
        private readonly IBitrixServiceSettings bitrixServiceSettings;
        
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Matcher matcher;
        

        public CoR(IBitrixServiceSettings _bitrixServiceSettings, string _token, IBitrixRestApi _bitrixRestApi, IUnitOfWork _uow, Matcher _matcher)
        {
	        
	        token = _token ?? throw new ArgumentNullException(nameof(_token));
	        bitrixApi = _bitrixRestApi ?? throw new ArgumentNullException(nameof(_bitrixRestApi));
	        uow = _uow ?? throw new ArgumentNullException(nameof(_uow));
	        matcher = _matcher ?? throw new ArgumentNullException(nameof(_matcher));
	        bitrixServiceSettings = _bitrixServiceSettings ?? throw new ArgumentNullException(nameof(_bitrixServiceSettings));
        }

        public async Task Process(uint dealId)
        {
	        try
	        {
		        var deal = await ProcessDeal(dealId);
		        if (deal == null){
			        logger.Error("Ошибка при получении сделки из BitrixApi");
			        return;
		        }

		        bool isSelfDelivery = deal.DeliveryType == "626";
		        bool isSmsPayment = deal.PaymentMethod == "1108";

		        //ищем у нас сделку по битрикс Id
		        needCreateOrder = !matcher.MatchOrderByBitrixId(uow, deal, out var ourOrder);
		        if (ourOrder != null){
			        logger.Info($"Сделка {deal.Id} найдена у нас под id: {ourOrder.Id}");
			        //TODO gavr вернуть 200
			        return;
		        } else if (deal.CreateInDV == 0)
		        {
			        logger.Info($"Сделка {deal.Id} имеет статус отличный от Завести в ДВ");
			        //TODO gavr вернуть 200
			        // return;
		        }
		        
		        Counterparty counterpartyForNewOrder = counterpartyForNewOrder =
			        await ProcessCounterparty(
				        deal); //TODO gavr потом добавить к ней адрес, когда он станет известен
		        
		        DeliverySchedule deliveryScheduleForOrder = null;
		        DeliveryPoint deliveryPointForOrder = null;
		        if (!isSelfDelivery){
			        needCreateNomenclature = false;
			        needSearchNomenclature = false;
			        deliveryPointForOrder = ProcessDeliveryPoint(deal, counterpartyForNewOrder);
			        deliveryScheduleForOrder = DeliveryScheduleRepository.GetByBitrixId(uow, deal.DeliverySchedule);
		        }


		        //точку доставки не ищем, её можно только начать сопоставлять тк кк она в сделке
		        //TODO удалить bitrixId у точек доставки из таблицы, сущности и маппинга 
		        var newOrder = ProcessOrder(
			        deal,
			        deliveryPointForOrder,
			        counterpartyForNewOrder, 
			        isSmsPayment,
			        deliveryScheduleForOrder
			    );
		        
		        var orderWithNomenclatures = await ProcessNomenclaturesAndAddToOrder(deal, newOrder);

		        
		        uow.Save(orderWithNomenclatures);
		        uow.Commit();
	        }
			catch (Exception e){
				logger.Error($"При обработке заказа с id {dealId} возникла ошибка: {e.Message}");
			}
			
        }


        class Sas {
	        private int a;
	        private string b;
	        private bool c;

	        public Sas(int a, string b, bool c)
	        {
		        this.a = a;
		        this.b = b ?? throw new ArgumentNullException(nameof(b));
		        this.c = c;
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
		        BottlesReturn = 0
		        
		        // Onli
	        };
	       
	        
	        return newOrder;

	    //
	    //     using (var uowForNewOrder = UnitOfWorkFactory.CreateWithNewRoot<Order>()){
	    //     
		   //       uowForNewOrder.Root.BitrixId = deal.Id;
		   //       uowForNewOrder.Root.PaymentType = deal.GetPaymentMethod();
		   //       uowForNewOrder.Root.CreateDate = deal.CreateDate;
		   //       uowForNewOrder.Root.DeliveryDate = deal.DeliveryDate;
		   //      
		   //       uowForNewOrder.Root.Client = counterpartyForNewOrder;
		   //       uowForNewOrder.Root.DeliveryPoint = deliveryPointForOrder;
		   //       uowForNewOrder.Root.OrderStatus = OrderStatus.Accepted; //TODO gavr уточнить
		   //       //Добавить OrderItems
		   //       foreach (var nomenclature in nomenclaturesForNewOrder){
					// uowForNewOrder.Root.AddAnyGoodsNomenclatureForSale(nomenclature);
		   //       }
		   //       
		   //       if (needCreateCounterParty)
			  //        uowForNewOrder.Root.SetFirstOrder(); //TODO gavr уточнить
		   //      
		   //       // uow.Save<Order>(newOrder);
		   //       uowForNewOrder.Save();
		   //       // uowForNewOrder.Commit(); //TODO gavr возможно сделать uowForNewCounterparty чайлдом глобального uow, если при коммите глобального коммитятся все его дети
		   //      
		   //       return uowForNewOrder.Root;
	    //     }

	        // return newOrder;
        }


        async Task<Deal> ProcessDeal(uint id)
        {
	        //Получаем сделку из битрикса
	        var deal = await bitrixApi.GetDealAsync(id);
			
	        //Определение это там контакт или компания
	        if (deal.CompanyId != 0)
		        isCompany = true;
	        else
		        isCompany = false;

	        return deal;
        }
        async Task<Counterparty> ProcessCounterparty(Deal deal)
        {
	        //Проверяем это клиент или компания
	        if (deal.CompanyId == null || deal.CompanyId == 0){
		        return await ProcessContact(deal);
	        }
	        else {
		        return await ProcessCompany((uint)deal.CompanyId);
	        }
        }

        async Task<Counterparty> ProcessContact(Deal deal)
        {
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
		        var newCounerparty = new Counterparty()
		        {
			        FullName = contact.SecondName + " " + contact.Name + " " + contact.LastName,
			        Name = contact.SecondName + " " + contact.Name + " " + contact.LastName,
			        BitrixId = contact.Id,
			        PersonType = PersonType.natural,
			        PaymentMethod = deal.GetPaymentMethod()
		        };
		        AddPhonesToCounterpartyFromContact(newCounerparty, contact);
		        
		        uow.Save(newCounerparty);
		        return newCounerparty;
	        }
        }

        async Task<Counterparty> ProcessCompany(uint companyId)
        {
	        //Получаем клиента из сделки
	        var company = await bitrixApi.GetCompany(companyId);
	        
	        //ищем у нас контакт или компанию по битрикс Id
	        if (matcher.MatchCounterpartyByBitrixId(uow, company.Id, out var matchedByBitrixId)){
		        return matchedByBitrixId;
	        }
	        else if (matcher.MatchCompanyPhoneAndName(uow, company, out var matchedByPhoneSecondName)){
		        return matchedByPhoneSecondName;
	        }
	        else{
		        //Create new
		        using (var uowForNewCounterparty = UnitOfWorkFactory.CreateWithNewRoot<Counterparty>()){
			        uowForNewCounterparty.Root.FullName = company.Title;
			        uowForNewCounterparty.Root.Name = string.Copy(uowForNewCounterparty.Root.FullName);
			        uowForNewCounterparty.Root.BitrixId = company.Id;
			        uowForNewCounterparty.Root.PersonType = PersonType.legal;
			        uowForNewCounterparty.Root.TypeOfOwnership = VodovozInfrastructure.Utils.NamesUtils.TryGetOrganizationType (company.Title) ?? "";
			        uowForNewCounterparty.Root.CreateDate = company.DateCreate;
			        uowForNewCounterparty.Root.IsArchive = false;
			        
			        uowForNewCounterparty.Save();
			        uowForNewCounterparty.Commit();
			        return uowForNewCounterparty.Root;
		        }
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
		        uow.Save(newPhone);
	        }
	        newCounterparty.Phones = phonesForNewCounterparty;
        }

        DeliveryPoint ProcessDeliveryPoint(Deal deal, Counterparty counterpartyForNewOrder)
        {
	        //Сопоставление точки доставки для клиента
	        DeliveryPoint deliveryPoint = null;
	        if (matcher.MatchDeliveryPoint(uow, deal, counterpartyForNewOrder, out deliveryPoint) ){
		        //Если к нам пришел незаполненный адресами контрпати, то привязываем ему адрес
		        if (counterpartyForNewOrder.DeliveryPoints.Count == 0){
			        counterpartyForNewOrder.DeliveryPoints.Add(deliveryPoint);
		        }
		        return deliveryPoint;
	        }
	        else{
		        //Create new DeliveryPoint
		        
		        using (var uowForNewCounterparty = UnitOfWorkFactory.CreateWithNewRoot<DeliveryPoint>()){
			        try{
				        uowForNewCounterparty.Root.RoomType = deal.GetRoomType();
			        }
			        catch (Exception e){
				        logger.Warn(e.Message);
				        logger.Warn($"Для заказа {deal.Id} не получилось сопоставить тип помещения '{deal.RoomType}' будет выставлен тип по умолчанию: помещение");
				        uowForNewCounterparty.Root.RoomType = RoomType.Room;
			        }
			        
			        uowForNewCounterparty.Root.Room = deal.RoomNumber;

			        try{
				        uowForNewCounterparty.Root.Building = deal.GetBuilding();
			        }
			        catch (Exception e){
				        logger.Warn($"Из заказа {deal.Id} не получилось получить номер дома '{deal.HouseAndBuilding}'");
				        logger.Warn(e.Message);
				        // uowForNewCounterparty.Root.Room = RoomType.Room;
			        }
			        uowForNewCounterparty.Root.Entrance = string.IsNullOrWhiteSpace(deal.Entrance)? "" : deal.Entrance;
			        uowForNewCounterparty.Root.Floor = string.IsNullOrWhiteSpace(deal.Floor)? "" : deal.Floor;;
			        uowForNewCounterparty.Root.EntranceType = deal.GetEntranceType();
			        uowForNewCounterparty.Root.City = deal.City;
			        uowForNewCounterparty.Root.Street = deal.DeliveryAddressWithoutHouse;

			        //Координаты
			        if (Matcher.TryParseCoordinates(deal.Coordinates, out var parsedLatitude, out var parsedLongitude))
				        uowForNewCounterparty.Root.SetСoordinates(parsedLatitude, parsedLongitude);
			        else
				        logger.Warn($"Неполучилось распарсить координаты {deal.Coordinates}");
			        
			        uowForNewCounterparty.Save();
			        uowForNewCounterparty.Commit();
			        
			        counterpartyForNewOrder.DeliveryPoints.Add(uowForNewCounterparty.Root);
			        return uowForNewCounterparty.Root;
		        }
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
			        // matchedNomenclatures.Add(ourNomenclature);
		        }
		        else if (matcher.MatchNomenclatureByName(uow, productBitrix.ProductName, out var outNomenclature)){
			        logger.Info($"Номенклатура {productBitrix.ProductName} сопоставилась имени");

			        // matchedNomenclatures.Add(outNomenclature);
			        newOrder.AddNomenclature(outNomenclature, 1);
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

        //TODO gavr сделать по той логике
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
			        newOrder.AddNomenclature(nomenclature, 1);
		        } else if (dealHasPromo && !needCreateNomenclature){
			        var discount = Math.Abs(nomenclature.GetPrice(1) - productBitrix.Price);
				    newOrder.AddNomenclature(nomenclature,1,discount,true);
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