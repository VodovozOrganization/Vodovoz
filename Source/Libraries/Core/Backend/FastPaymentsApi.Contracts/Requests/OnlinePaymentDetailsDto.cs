using System.Text.Json.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	public class OnlinePaymentDetailsDto
	{
		[JsonPropertyName("id")]
		public int OnlineOrderId { get; set; }
		[JsonPropertyName("amount")]
		public OnlinePaymentSumDetailsDto PaymentSumDetails { get; set; }
	}
}
