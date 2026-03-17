using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	/// <summary>
	/// Модель количества товара в формате Yandex Pay
	/// </summary>
	public class YandexPayQuantity
	{
		/// <summary>
		/// Количество товара
		/// </summary>
		[JsonPropertyName("count")]
		public string Count { get; set; }
	}
}
