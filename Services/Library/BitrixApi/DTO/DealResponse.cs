using Newtonsoft.Json;

namespace BitrixApi.DTO
{
	public class DealResponse
    {
        [JsonProperty("result")] 
		public Deal Result { get; set; }

        [JsonProperty("time")] 
		public ResponseTime ResponseTime { get; set; }
    }
}