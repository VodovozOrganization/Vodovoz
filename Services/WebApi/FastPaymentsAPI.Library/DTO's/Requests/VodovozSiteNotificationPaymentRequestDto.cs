using System.Text.Json.Serialization;

namespace FastPaymentsAPI.Library.DTO_s.Requests
{
	public class VodovozSiteNotificationPaymentRequestDto
	{
		[JsonPropertyName("type")]
		public string MessageType => "notification";
		[JsonPropertyName("event")]
		public string PaymentStatus { get; set; }
		[JsonPropertyName("object")]
		public OnlinePaymentDetailsDto PaymentDetails { get; set; }
	}

	public class OnlinePaymentDetailsDto
	{
		[JsonPropertyName("id")]
		public int OnlineOrderId { get; set; }
		[JsonPropertyName("amount")]
		public OnlinePaymentSumDetailsDto PaymentSumDetails { get; set; }
	}

	public class OnlinePaymentSumDetailsDto
	{
		[JsonPropertyName("value")]
		public decimal PaymentSum { get; set; }
		[JsonPropertyName("currency")]
		public string Currency => CurrencyType.RUB.ToString();
	}

	public enum VodovozSitePaymentStatus
	{
		canceled,
		succeeded
	}

	public enum CurrencyType
	{
		RUB
	}
}
