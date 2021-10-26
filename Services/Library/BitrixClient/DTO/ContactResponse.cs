using Newtonsoft.Json;

namespace Bitrix.DTO
{
	public class ContactResponse
	{
		[JsonProperty("result")]
		public Contact Result { get; set; }

		[JsonProperty("time")]
		public ResponseTime ResponseTime { get; set; }
	}
}
