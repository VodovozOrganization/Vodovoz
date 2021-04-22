using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class CustomFieldsDealList
    {
        [JsonProperty("result")] 
        public IList<CustomFieldFromList> Result { get; set; }
        
        [JsonProperty("time")] 
        public ResponseTime ResponseTime { get; set; }
    }
    
    public class CustomFieldFromList
    {
        [JsonProperty("ID")]  
        public int Id { get; set; }
        
        [JsonProperty("FIELD_NAME")]  
        public string FieldName { get; set; }
        
        [JsonProperty("USER_TYPE_ID")]  
        public string UserTypeId { get; set; }
    }
}