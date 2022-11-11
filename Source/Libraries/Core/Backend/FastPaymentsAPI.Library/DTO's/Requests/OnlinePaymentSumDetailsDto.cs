using System.Text.Json.Serialization;

namespace FastPaymentsAPI.Library.DTO_s.Requests
{
	public class OnlinePaymentSumDetailsDto
	{
		[JsonPropertyName("value")]
		public decimal PaymentSum { get; set; }
		[JsonPropertyName("currency")]
		public string Currency => CurrencyType.RUB.ToString();
	}
	
	public enum CurrencyType
	{
		RUB
	}
}
