using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	public class YandexPayQuantity
	{
		[JsonPropertyName("count")]
		public string Count { get; set; }
	}
}
