using Newtonsoft.Json;

namespace Bitrix.DTO 
{
    public class ChangeStatusResult 
    {
		[JsonProperty("result")]
		public bool Result { get; set; }
        
		[JsonProperty("time")]
		public ResponseTime ResponseTime { get; set; }
    }
}