using System.Text.Json.Serialization;

namespace FastPaymentsAPI.Library.DTO_s.Requests
{
	public class OnlinePaymentDetailsDto
	{
		[JsonPropertyName("id")]
		public int OnlineOrderId { get; set; }
		[JsonPropertyName("amount")]
		public OnlinePaymentSumDetailsDto PaymentSumDetails { get; set; }
	}
}
