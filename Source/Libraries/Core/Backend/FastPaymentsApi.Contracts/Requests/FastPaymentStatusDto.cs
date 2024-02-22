using System.Text.Json.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	public class FastPaymentStatusDto
	{
		[JsonPropertyName("status")]
		public RequestPaymentStatus PaymentStatus { get; set; }
		[JsonPropertyName("details")]
		public OnlinePaymentDetailsDto PaymentDetails { get; set; }
	}
}
