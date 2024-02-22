using System.Text.Json.Serialization;

namespace FastPaymentsApi.Contracts.Requests
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
