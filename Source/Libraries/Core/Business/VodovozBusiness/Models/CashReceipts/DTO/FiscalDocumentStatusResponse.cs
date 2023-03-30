using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Vodovoz.Models.CashReceipts.DTO
{
	public class FiscalFailureInfo
    {
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }
	}
}
