using System;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	public class YooKassaPaymentResponse
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("amount")]
		public YooKassaAmount Amount { get; set; }

		[JsonPropertyName("refunded_amount")]
		public YooKassaAmount RefundedAmount { get; set; }

		[JsonPropertyName("captured_at")]
		public DateTime? CapturedAt { get; set; }

		[JsonPropertyName("test")]
		public bool Test { get; set; }
	}
}
