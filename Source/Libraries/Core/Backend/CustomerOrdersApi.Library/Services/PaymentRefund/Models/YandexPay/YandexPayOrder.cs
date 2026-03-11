using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	/// <summary>
	/// Модель заказа из Яндекс Пэй
	/// </summary>
	public class YandexPayOrder
	{
		/// <summary>
		/// ID заказа в Яндекс Пэй
		/// </summary>
		[JsonPropertyName("orderId")]
		public string OrderId { get; set; }

		/// <summary>
		/// Сумма заказа
		/// </summary>
		[JsonPropertyName("orderAmount")]
		public string OrderAmount { get; set; }

		/// <summary>
		/// Статус платежа
		/// </summary>
		[JsonPropertyName("paymentStatus")]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public YandexPayPaymentStatus PaymentStatus { get; set; }
	}
}
