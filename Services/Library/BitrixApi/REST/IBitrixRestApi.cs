using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BitrixApi.DTO;

namespace BitrixApi.REST
{
    public interface IBitrixRestApi
    {
        //crm.deal.get
        Task<Deal> GetDealAsync(uint id);

        //crm.contact.get
        Contact GetContact(uint id);
        
        //crm.company.get
        Company GetCompany(uint id);
        
        //crm.product.get
        Task<Product> GetProduct(uint id);
        
        //crm.deal.productrows.get
        Task<IList<DealProductItem>> GetProductsForDeal(uint dealId);

		//Task<IList<uint>> GetDealsIdsBetweenDates(DateTime date1, DateTime date2);

		IList<Deal> GetDeals(DateTime dateTimeFrom, DateTime dateTimeTo);
        
		Task<bool> SendWONBitrixStatus(uint bitrixId);


        #region CustomFields
		/*
        //crm.deal.userfield.list
        public Task<IList<CustomFieldFromList>> GetAllCustomFieldsFromDeal();
        
        //crm.deal.userfield.get
        public Task<CustomField> GetCustomFieldDeal(int id);

        public Task<Dictionary<string, CustomField>> GetMapCustomFieldsShitNamesToRus();

        public string SerializeCustomFieldsShitToRusNamesToFile(Dictionary<string, CustomField> ShitToRusNames);

        public Task CreateShitToRusCustomFieldsFile(string filename);
		*/
        #endregion CustomFields
    }
}