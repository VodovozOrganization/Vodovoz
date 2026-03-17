using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	/// <summary>
	/// Модель товара в корзине Yandex Pay
	/// </summary>
	public class YandexPayCartItem
	{
		/// <summary>
		/// Идентификатор товара
		/// </summary>
		[JsonPropertyName("productId")]
		public string ProductId { get; set; }

		/// <summary>
		/// Количество товара
		/// </summary>
		[JsonPropertyName("quantity")]
		public YandexPayQuantity Quantity { get; set; }

		/// <summary>
		/// Общая стоимость товара (с учетом количества)
		/// </summary>
		[JsonPropertyName("total")]
		public string Total { get; set; }
	}
}
