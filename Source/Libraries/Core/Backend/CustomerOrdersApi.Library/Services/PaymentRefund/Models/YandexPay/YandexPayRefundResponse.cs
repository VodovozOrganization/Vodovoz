using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	/// <summary>
	/// Ответ от Yandex Pay API при создании возврата
	/// </summary>
	public class YandexPayRefundResponse
	{
		/// <summary>
		/// Информация об операции возврата
		/// </summary>
		[JsonPropertyName("operation")]
		public YandexPayRefundOperation Operation { get; set; }
	}
}
