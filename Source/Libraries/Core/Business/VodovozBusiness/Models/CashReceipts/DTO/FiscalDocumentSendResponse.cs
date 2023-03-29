using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Vodovoz.Models.CashReceipts.DTO
{
    public class FiscalDocumentInfoResponse
    {
		[JsonProperty("status")]
		[JsonConverter(typeof(StringEnumConverter))]
		public FiscalDocumentStatus Status { get; set; }

		[JsonProperty("fnState")]
		public string FnState { get; set; }

		[JsonProperty("fiscalInfo")]
		public FiscalInfo FiscalInfo { get; set; }

		[JsonProperty("failureInfo")]
		public FiscalFailureInfo FailureInfo { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("timeStatusChanged")]
		public string TimeStatusChangedString { get; set; }
	}
}
