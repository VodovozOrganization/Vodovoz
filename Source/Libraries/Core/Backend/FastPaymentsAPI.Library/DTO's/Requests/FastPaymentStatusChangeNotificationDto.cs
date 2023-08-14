using System.Text.Json.Serialization;

namespace FastPaymentsAPI.Library.DTO_s.Requests
{
	public class FastPaymentStatusChangeNotificationDto
	{
		[JsonPropertyName("type")]
		public string MessageType => "notification";
		[JsonPropertyName("event")]
		public PaymentStatusNotification PaymentStatus { get; set; }
		[JsonPropertyName("object")]
		public OnlinePaymentDetailsDto PaymentDetails { get; set; }
	}
}
