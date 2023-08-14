using System.Text.Json.Serialization;

namespace FastPaymentsAPI.Library.DTO_s.Requests
{
	public class FastPaymentStatusDto
	{
		[JsonPropertyName("status")]
		public RequestPaymentStatus PaymentStatus { get; set; }
		[JsonPropertyName("details")]
		public OnlinePaymentDetailsDto PaymentDetails { get; set; }
	}
}
