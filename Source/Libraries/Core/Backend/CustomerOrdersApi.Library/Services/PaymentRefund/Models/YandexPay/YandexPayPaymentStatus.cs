using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	/// <summary>
	/// Статусы платежа в Яндекс Пэй
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum YandexPayPaymentStatus
	{
		/// <summary>
		/// Платеж авторизован, средства заблокированы
		/// </summary>
		[JsonPropertyName("AUTHORIZED")]
		Authorized,

		/// <summary>
		/// Заказ успешно оплачен, средства списаны
		/// </summary>
		[JsonPropertyName("CAPTURED")]
		Captured,

		/// <summary>
		/// Частичный возврат средств
		/// </summary>
		[JsonPropertyName("PARTIALLY_REFUNDED")]
		PartiallyRefunded,

		/// <summary>
		/// Полный возврат средств
		/// </summary>
		[JsonPropertyName("REFUNDED")]
		Refunded,

		/// <summary>
		/// Платеж отменен
		/// </summary>
		[JsonPropertyName("CANCELLED")]
		Cancelled
	}
}
