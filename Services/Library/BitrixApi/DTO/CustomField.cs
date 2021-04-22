using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class CustomField
    {
        [JsonProperty("ID")]  
        public int Id { get; set; }
        
        [JsonProperty("FIELD_NAME")]  
        public string FieldName { get; set; }
        
        [JsonProperty("USER_TYPE_ID")]  
        public string UserTypeId { get; set; }
        
        [JsonProperty("EDIT_FORM_LABEL")]  
        public RussianFieldName RussianName { get; set; }
    }
}