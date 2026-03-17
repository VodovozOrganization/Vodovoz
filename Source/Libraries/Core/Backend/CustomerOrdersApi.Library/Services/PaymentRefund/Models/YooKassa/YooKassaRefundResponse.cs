using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	/// <summary>
	/// Ответ от API ЮKassa при создании возврата
	/// </summary>
	public class YooKassaRefundResponse
	{
		/// <summary>
		/// Идентификатор возврата в ЮKassa
		/// </summary>
		[JsonPropertyName("id")]
		public string Id { get; set; }

		/// <summary>
		/// Статус возврата
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Сумма возврата
		/// </summary>
		[JsonPropertyName("amount")]
		public YooKassaAmount Amount { get; set; }

		/// <summary>
		/// Время создания возврата
		/// </summary>
		[JsonPropertyName("created_at")]
		public DateTime CreatedAt { get; set; }

		/// <summary>
		/// Идентификатор платежа, по которому был совершен возврат
		/// </summary>
		[JsonPropertyName("payment_id")]
		public string PaymentId { get; set; }

		/// <summary>
		/// Детали отмены возврата
		/// </summary>
		[JsonPropertyName("cancellation_details")]
		public YooKassaCancellationDetails CancellationDetails { get; set; }

		/// <summary>
		/// Метаданные возврата
		/// </summary>
		[JsonPropertyName("metadata")]
		public Dictionary<string, string> Metadata { get; set; }
	}
}
