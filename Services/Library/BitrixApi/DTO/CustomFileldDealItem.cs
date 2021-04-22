using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class CustomFileldDealItem
    {
        [JsonProperty("result")] 
        public CustomField Result { get; set; }
        
        [JsonProperty("time")] 
        public ResponseTime ResponseTime { get; set; }
    }
    
    

    public class RussianFieldName
    {
        [JsonProperty("ru")]  public string Name { get; set; }
    }
}