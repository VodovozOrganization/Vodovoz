using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BitrixApi.DTO;
using BitrixApi.REST;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
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
        private IBitrixRestApi bitrixApi;
        private IUnitOfWork uow;
        
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public CoR(string _token, IBitrixRestApi _bitrixRestApi, IUnitOfWork _uow)
        {
	        token = _token ?? throw new ArgumentNullException(nameof(_token));
	        bitrixApi = _bitrixRestApi ?? throw new ArgumentNullException(nameof(_bitrixRestApi));
	        uow = _uow ?? throw new ArgumentNullException(nameof(_uow));
        }

		//main func
        public async Task Process(uint id)
        {
	        
			
			var deal = await ProcessOrder(id);
			if (deal == null){
				logger.Error("Ошибка при получении сделки из BitrixApi");
				return;
			} 
			
			
			//ищем у нас сделку по битрикс Id
			needCreateOrder = !Matcher.MatchOrderByBitrixId(deal, out var ourOrder);
			if (ourOrder != null){
				//TODO gavr вернуть 200
				return;
			}
			
			//TODO gavr !!! сопоставляем точку доставки только если это не самовызов
			var counterpartyForNewOrder = await ProcessCounterparty(deal); //TODO gavr потом добавить к ней адрес, когда он станет известен
			var nomenclaturesForNewOrder = await ProcessNomenclatures(deal);


			
			//точку доставки не ищем, её можно только начать сопоставлять тк кк она в сделке
			//TODO удалить bitrixId у точек доставки из таблицы, сущности и маппинга 
			
			//Сопоставление точки доставки для клиента
			DeliveryPoint deliveryPoint = null;
			Matcher.MatchDeliveryPoint(deal, counterpartyForNewOrder, out deliveryPoint);
			
        }
        
        
        async Task<Deal> ProcessOrder(uint id)
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
	        if (Matcher.MatchCounterpartyByBitrixId(contact.Id, out var matchedByBitrixId)){
		        return matchedByBitrixId;
	        }
	        else if (Matcher.MatchCounterpartyByPhoneAndSecondName(contact, out var matchedByPhoneSecondName)){
		        return matchedByPhoneSecondName;
	        }
	        else{
		        //Create new
		        using (var uowForNewCounterparty = UnitOfWorkFactory.CreateWithNewRoot<Counterparty>()){
			        
			        uowForNewCounterparty.Root.FullName = contact.SECOND_NAME + " " + contact.NAME + " " + contact.LAST_NAME;
			        uowForNewCounterparty.Root.Name = string.Copy(uowForNewCounterparty.Root.FullName);
			        uowForNewCounterparty.Root.BitrixId = contact.Id;
			        uowForNewCounterparty.Root.PersonType = PersonType.legal;
			        // uowForNewCounterparty.Root.PaymentMethod = ; //TODO gavr paymentMethod
				        
			        //Конвертация телефонов из дто в нормальные
			        IList<Phone> phonesForNewCounterparty = new List<Phone>();
			        foreach (var phone in contact.PHONE){
				        var uowForNewPhone = UnitOfWorkFactory.CreateWithNewChildRoot<Phone>(uowForNewCounterparty);
				        uowForNewPhone.Root.Number = phone.VALUE;
				        phonesForNewCounterparty.Add(uowForNewPhone.Root);
				        uowForNewPhone.Save();
			        }

			        uowForNewCounterparty.Root.Phones = phonesForNewCounterparty;
			        
			        uowForNewCounterparty.Save();
			        uowForNewCounterparty.Commit(); //TODO gavr возможно сделать uowForNewCounterparty чайлдом глобального uow, если при коммите глобального коммитятся все его дети
			        return uowForNewCounterparty.Root;
		        }
	        }
        }

        async Task<Counterparty> ProcessCompany(uint companyId)
        {
	        
	        //Получаем клиента из сделки
	        var company = await bitrixApi.GetCompany(companyId);
	        
	        //ищем у нас контакт или компанию по битрикс Id
	        if (Matcher.MatchCounterpartyByBitrixId(company.Id, out var matchedByBitrixId)){
		        return matchedByBitrixId;
	        }
	        else if (Matcher.MatchCompanyPhoneAndName(company, out var matchedByPhoneSecondName)){
		        return matchedByPhoneSecondName;
	        }
	        else{
		        //Create new
		        using (var uowForNewCounterparty = UnitOfWorkFactory.CreateWithNewRoot<Counterparty>()){
			      //TODO gavr сделать созданние компании
			      throw new NotImplementedException();
		        }
	        }
        }
        
        
		
        async Task testDeliveryPoint()
        {
			
        }
		
        async Task<IList<Nomenclature>> ProcessNomenclatures(Deal deal)
        {
	        var productList = await bitrixApi.GetProductsForDeal(deal.Id);
	        //ищем у нас товары по битрикс Id
	        IList<Nomenclature> matchedNomenclatures = new List<Nomenclature>();
	        IList<ProductFromDeal> unmatchedProducts = new List<ProductFromDeal>();
			
	        foreach (var productBitrix in productList){
		        if (Matcher.MatchNomenclatureByBitrixId(productBitrix.Id, out var ourNomenclature)){
			        matchedNomenclatures.Add(ourNomenclature);
		        }
		        else{
			        unmatchedProducts.Add(productBitrix);
		        }
	        }

	        if (unmatchedProducts.Count != 0)
		        needCreateNomenclature = true;
        }
    }
}