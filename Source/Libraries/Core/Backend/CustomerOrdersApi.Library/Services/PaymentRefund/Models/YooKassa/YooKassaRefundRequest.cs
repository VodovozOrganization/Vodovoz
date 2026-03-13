using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	public class YooKassaRefundRequest
	{
		[JsonPropertyName("amount")]
		public YooKassaAmount Amount { get; set; }

		[JsonPropertyName("payment_id")]
		public string PaymentId { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("metadata")]
		public Dictionary<string, string> Metadata { get; set; }
	}
}
