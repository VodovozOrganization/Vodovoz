using Newtonsoft.Json;

namespace BitrixApi.DTO
{
	public class ContactRequest
    {
        [JsonProperty("result")]
        public Contact Result { get; set; }
        
        [JsonProperty("time")] 
        public ResponseTime ResponseTime { get; set; }
    }
}