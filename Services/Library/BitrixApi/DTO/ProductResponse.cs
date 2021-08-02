using Newtonsoft.Json;

namespace BitrixApi.DTO
{
    public class ProductResponse
    {
        [JsonProperty("result")] 
		public Product Result { get; set; }

        [JsonProperty("time")] 
		public ResponseTime ResponseTime { get; set; }
    }
}