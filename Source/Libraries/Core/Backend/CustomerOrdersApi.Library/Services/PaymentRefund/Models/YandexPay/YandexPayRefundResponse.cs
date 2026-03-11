using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	public class YandexPayRefundResponse
	{
		[JsonPropertyName("operation")]
		public YandexPayRefundOperation Operation { get; set; }
	}
}
