using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bitrix.DTO
{
    public class Contact
    {
        [JsonProperty("ID")] 
        public uint Id { get; set; }
        
        [JsonProperty("COMMENTS")] 
        public string Comments { get; set; }
        
        [JsonProperty("NAME")] 
        public string Name { get; set; }
        
        [JsonProperty("SECOND_NAME")] 
        public string SecondName { get; set; }
        
        [JsonProperty("LAST_NAME")] 
        public string LastName { get; set; }
        
        [JsonProperty("LEAD_ID")] 
        public string LeadId { get; set; }
        
        [JsonProperty("TYPE_ID")] 
        public string TypeId { get; set; }
        
        [JsonProperty("SOURCE_ID")] 
        public string SourceId { get; set; }
        
        [JsonProperty("COMPANY_ID")] 
        public string CompanyId { get; set; }
        
        [JsonProperty("ASSIGNED_BY_ID")] 
        public int AssignedById { get; set; }
        
        [JsonProperty("CREATED_BY_ID")] 
        public int CreatedById { get; set; }
        
        [JsonProperty("DATE_CREATE")] 
        public DateTime CreatedDate { get; set; }
        
        
        [JsonProperty("MODIFY_BY_ID")] 
        public int ModifyById { get; set; }
        
        [JsonProperty("OPENED")] 
        public string Opened { get; set; }
        
        [JsonProperty("PHONE")] 
        public IList<Phone> Phones { get; set; }
    }
}