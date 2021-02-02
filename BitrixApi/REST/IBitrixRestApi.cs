using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitrixApi.DTO;
using Newtonsoft.Json;

namespace BitrixApi.REST
{
    public interface IBitrixRestApi
    {
       
        
        //crm.deal.get
        public Task<Deal> GetDealAsync(uint id);
        // public Task<Deal> GetDeal(uint id);
        
        //crm.contact.get
        public Task<Contact> GetContact(uint id);
        
        //crm.company.get
        public Task<Company> GetCompany(uint id);

        
        //crm.product.get
        public Task<Product> GetProduct(uint id);


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