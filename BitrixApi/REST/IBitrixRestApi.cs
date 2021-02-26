using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitrixApi.DTO;
using Newtonsoft.Json;
using QS.DomainModel.UoW;

namespace BitrixApi.REST
{
    public interface IBitrixRestApi
    {
       
        
        //crm.deal.get
        Task<Deal> GetDealAsync(uint id);
        // public Task<Deal> GetDeal(uint id);
        
        //crm.contact.get
        Task<Contact> GetContact(uint id);
        
        //crm.company.get
        Task<Company> GetCompany(uint id);

        
        //crm.product.get
        Task<Product> GetProduct(uint id);
        
        //crm.deal.productrows.get
        Task<IList<ProductFromDeal>> GetProductsForDeal(uint dealId);
        
        Task<IList<uint>> GetDealsIdsBetweenDates(IUnitOfWork uow, DateTime date1, DateTime date2);


        #region CustomFields

        //crm.deal.userfield.list
        public Task<IList<CustomFieldFromList>> GetAllCustomFieldsFromDeal();
        
        //crm.deal.userfield.get
        public Task<CustomField> GetCustomFieldDeal(int id);

        public Task<Dictionary<string, CustomField>> GetMapCustomFieldsShitNamesToRus();

        public string SerializeCustomFieldsShitToRusNamesToFile(Dictionary<string, CustomField> ShitToRusNames);

        public Task CreateShitToRusCustomFieldsFile(string filename);

        #endregion CustomFields
    }
}