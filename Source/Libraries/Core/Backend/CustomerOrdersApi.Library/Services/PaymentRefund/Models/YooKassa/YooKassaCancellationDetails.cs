using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	public class YooKassaCancellationDetails
	{
		[JsonPropertyName("party")]
		public string Party { get; set; }

		[JsonPropertyName("reason")]
		public string Reason { get; set; }
	}
}
