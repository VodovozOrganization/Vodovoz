using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	public class YooKassaRefundResponse
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("amount")]
		public YooKassaAmount Amount { get; set; }

		[JsonPropertyName("created_at")]
		public DateTime CreatedAt { get; set; }

		[JsonPropertyName("payment_id")]
		public string PaymentId { get; set; }

		[JsonPropertyName("cancellation_details")]
		public YooKassaCancellationDetails CancellationDetails { get; set; }

		[JsonPropertyName("metadata")]
		public Dictionary<string, string> Metadata { get; set; }
	}
}
