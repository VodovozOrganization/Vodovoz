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
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Parameters;
using Contact = BitrixApi.DTO.Contact;
using Phone = Vodovoz.Domain.Contacts.Phone;

namespace BitrixIntegration {
    public class CoR {
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
        
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Matcher matcher;
        

        public CoR(string _token, IBitrixRestApi _bitrixRestApi, IUnitOfWork _uow, Matcher _matcher)
        {
	        token = _token ?? throw new ArgumentNullException(nameof(_token));
	        bitrixApi = _bitrixRestApi ?? throw new ArgumentNullException(nameof(_bitrixRestApi));
	        uow = _uow ?? throw new ArgumentNullException(nameof(_uow));
	        matcher = _matcher ?? throw new ArgumentNullException(nameof(_matcher));
        }

		//main func
        public async Task Process(uint id)
        {
	        try
	        {
		        var deal = await ProcessDeal(id);
		        if (deal == null){
			        logger.Error("Ошибка при получении сделки из BitrixApi");
			        return;
		        }

		        //ищем у нас сделку по битрикс Id
		        needCreateOrder = !matcher.MatchOrderByBitrixId(uow, deal, out var ourOrder);
		        if (ourOrder != null){
			        //TODO gavr вернуть 200
			        return;
		        }
		        
		        Counterparty counterpartyForNewOrder = null;
		        if (deal.DeliveryType != "626"){
			        needCreateNomenclature = false;
			        needSearchNomenclature = false;
			        counterpartyForNewOrder =
				        await ProcessCounterparty(
					        deal); //TODO gavr потом добавить к ней адрес, когда он станет известен
		        }
		        
		        var deliveryPointForOrder = ProcessDeliveryPoint(deal, counterpartyForNewOrder);
		        
		        var nomenclaturesForNewOrder = await ProcessNomenclatures(deal);
		        
		        
		        
		        //точку доставки не ищем, её можно только начать сопоставлять тк кк она в сделке
		        //TODO удалить bitrixId у точек доставки из таблицы, сущности и маппинга 
		        var newOrder = ProcessOrder(deal, deliveryPointForOrder, counterpartyForNewOrder, nomenclaturesForNewOrder);
		        uow.Save(newOrder);
		        uow.Commit();

	        }
			catch (Exception e){
				logger.Error($"При обработке заказа с id {id} возникла ошибка: {e.Message}");
			}
			
        }

        private Order ProcessOrder(Deal deal, DeliveryPoint deliveryPointForOrder, Counterparty counterpartyForNewOrder, IList<Nomenclature> nomenclaturesForNewOrder)
        {
	        // var newOrder = new Order()
	        // {
		       //  BitrixId = deal.Id,
		       //  PaymentType = deal.GetPaymentMethod(),
		       //  CreateDate = deal.CreateDate,
		       //  DeliveryDate = deal.DeliveryDate, 
		       //  Client = counterpartyForNewOrder,
		       //  DeliveryPoint = deliveryPointForOrder,
		       //  OrderStatus = OrderStatus.Accepted
	        // };
	        //
	        // foreach (var nomenclature in nomenclaturesForNewOrder){
		       //  newOrder.AddAnyGoodsNomenclatureForSale(nomenclature);
	        // }
	        //
	        // return newOrder;


	        using (var uowForNewOrder = UnitOfWorkFactory.CreateWithNewRoot<Order>()){
	        
		         uowForNewOrder.Root.BitrixId = deal.Id;
		         uowForNewOrder.Root.PaymentType = deal.GetPaymentMethod();
		         uowForNewOrder.Root.CreateDate = deal.CreateDate;
		         uowForNewOrder.Root.DeliveryDate = deal.DeliveryDate;
		        
		         uowForNewOrder.Root.Client = counterpartyForNewOrder;
		         uowForNewOrder.Root.DeliveryPoint = deliveryPointForOrder;
		         uowForNewOrder.Root.OrderStatus = OrderStatus.Accepted; //TODO gavr уточнить
		         //Добавить OrderItems
		         foreach (var nomenclature in nomenclaturesForNewOrder){
					uowForNewOrder.Root.AddAnyGoodsNomenclatureForSale(nomenclature);
		         }
		         
		         if (needCreateCounterParty)
			         uowForNewOrder.Root.SetFirstOrder(); //TODO gavr уточнить
		        
		         // uow.Save<Order>(newOrder);
		         uowForNewOrder.Save();
		         // uowForNewOrder.Commit(); //TODO gavr возможно сделать uowForNewCounterparty чайлдом глобального uow, если при коммите глобального коммитятся все его дети
		        
		         return uowForNewOrder.Root;
	        }

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
	        
	        //ищем у нас контакт или компанию по битрикс Id
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
		        
		        //Create new
		        // using (var uowForNewCounterparty = UnitOfWorkFactory.CreateWithNewRoot<Counterparty>()){
			       //  
			       //  uowForNewCounterparty.Root.FullName = contact.SecondName + " " + contact.Name + " " + contact.LastName;
			       //  uowForNewCounterparty.Root.Name = string.Copy(uowForNewCounterparty.Root.FullName);
			       //  uowForNewCounterparty.Root.BitrixId = contact.Id;
			       //  uowForNewCounterparty.Root.PersonType = PersonType.natural;
			       //  uowForNewCounterparty.Root.PaymentMethod = deal.GetPaymentMethod(); 
			       //  AddPhonesToCounterpartyFromContact(uowForNewCounterparty, contact);
			       //  
			       //  uowForNewCounterparty.Save();
			       //  // uowForNewCounterparty.Commit();
			       //  return uowForNewCounterparty.Root;
		        // }
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
				        logger.Warn(e.Message);
				        logger.Warn($"Из заказа {deal.Id} не получилось получить номер дома '{deal.HouseAndBuilding}'");
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

        async Task AddDeliveryPointsToCounterPartyFromCompany(Counterparty counterparty, Company company)
        {
	        
        }
        
        async Task AddDeliveryPointsToCounterPartyFromContact(Counterparty counterparty, Contact contact)
        {
	        
        }


        enum NomenclatureUpdateScript {
	        UpdatePriceFromBitrix,
	        UpdatePriceFromBitrixWithDiscount,
	        CreateNew
        }
        async Task<IList<Nomenclature>> ProcessNomenclatures(Deal deal)
        {
	       
	        bool isOurProduct = false; // TODO gavr определять это из товара когда это там появиться 
	        var productList = await bitrixApi.GetProductsForDeal(deal.Id);
	        //ищем у нас товары по битрикс Id
	        IList<Nomenclature> matchedNomenclatures = new List<Nomenclature>();
	        IList<ProductFromDeal> unmatchedProducts = new List<ProductFromDeal>();
			
	        foreach (var productBitrix in productList){
		        if (matcher.MatchNomenclatureByBitrixId(uow, productBitrix.Id, out var ourNomenclature)){

			        // UpdateNomenclaturePrice(productBitrix, ourNomenclature);
			        //TODO gavr обновление цен номенклатур
			        
			        matchedNomenclatures.Add(ourNomenclature);
		        }
		        else if (matcher.MatchNomenclatureByName(uow, productBitrix.PRODUCT_NAME, out var outNomenclature)){
			        matchedNomenclatures.Add(outNomenclature);
		        }
		        else{
			        unmatchedProducts.Add(productBitrix);
		        }
	        }

	        if (unmatchedProducts.Count != 0){
		        // needCreateNomenclature = true;
		        // using (var uowForNewNomenclatures = UnitOfWorkFactory.CreateWithNewRoot<Nomenclature>()){
			       //  uowForNewNomenclatures.Root. = deal.GetRoomType();
		        //
		        //
			       //  uowForNewNomenclatures.Save();
			       //  uowForNewNomenclatures.Commit();
			       //  
			       //  counterpartyForNewOrder.DeliveryPoints.Add(uowForNewNomenclatures.Root);
			       //  return uowForNewNomenclatures.Root;
			       throw new NotImplementedException();
			       // }
	        }



	        return matchedNomenclatures;
        }

        //TODO gavr сделать по той логике
        /*
	        *	1) если UF_CRM_1596187803 null то мы обновляем цену товара в ДВ на ту что пришла из сделки
			   2) если UF_CRM_1596187803 не null и товар сопоставился по названию или bitrix id, то отнимаем от цены ДВ цену 
				  которая пришла и проставляем это значение как скидку в рублях
			   3) Если товара нет в базе, то создаем его заполняя данными битрикса
	        */
        private void UpdateNomenclaturePrice(IUnitOfWork uow, bool isOurProduct, Deal deal, ProductFromDeal productBitrix, Nomenclature ourNomenclature)
        {
	        throw new NotImplementedException();
	        if (!isOurProduct && string.IsNullOrWhiteSpace(deal.Promocode)){
		        // ourNomenclature.NomenclaturePrice = ;
	        }
        }
    }
}