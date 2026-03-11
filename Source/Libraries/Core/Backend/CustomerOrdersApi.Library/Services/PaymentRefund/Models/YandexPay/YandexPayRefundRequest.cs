using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	/// <summary>
	/// Запрос на возврат средств в Яндекс Пэй
	/// </summary>
	public record YandexPayRefundRequest
	{
		/// <summary>
		/// Сумма возврата
		/// </summary>
		[JsonPropertyName("refundAmount")]
		public decimal RefundAmount { get; set; }

		/// <summary>
		/// Внешний ID операции (для идемпотентности)
		/// </summary>
		[JsonPropertyName("externalOperationId")]
		public string ExternalOperationId { get; set; }

		/// <summary>
		/// Целевая корзина после возврата
		/// </summary>
		[JsonPropertyName("targetCart")]
		public YandexPayTargetCart TargetCart { get; set; }

		/// <summary>
		/// ID заказа в Яндекс Пэй
		/// </summary>
		[JsonIgnore]
		public string OrderId { get; set; }
	}
}
