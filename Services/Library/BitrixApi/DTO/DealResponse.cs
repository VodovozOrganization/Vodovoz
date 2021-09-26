using Newtonsoft.Json;

namespace Bitrix.DTO
{
	public class DealResponse
    {
        [JsonProperty("result")] 
		public Deal Result { get; set; }

        [JsonProperty("time")] 
		public ResponseTime ResponseTime { get; set; }
    }
}