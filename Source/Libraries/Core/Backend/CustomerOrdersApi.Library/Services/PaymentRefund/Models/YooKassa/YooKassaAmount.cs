using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	public class YooKassaAmount
	{
		[JsonPropertyName("value")]
		public string Value { get; set; }

		[JsonPropertyName("currency")]
		public string Currency { get; set; } = "RUB";
	}
}
