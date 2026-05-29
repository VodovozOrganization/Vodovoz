using System;
using System.Text.Json.Serialization;
using YooKassaApi.Library.Models;

namespace YooKassaApi.Library.Responses
{
	/// <summary>
	/// Ответ от API ЮKassa с информацией о платеже
	/// </summary>
	public class YooKassaPaymentResponse
	{
		/// <summary>
		/// Идентификатор платежа в ЮKassa
		/// </summary>
		[JsonPropertyName("id")]
		public string Id { get; set; }

		/// <summary>
		/// Статус платежа
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Сумма платежа
		/// </summary>
		[JsonPropertyName("amount")]
		public YooKassaAmount Amount { get; set; }

		/// <summary>
		/// Возвращенная сумма
		/// </summary>
		[JsonPropertyName("refunded_amount")]
		public YooKassaAmount RefundedAmount { get; set; }

		/// <summary>
		/// Время подтверждения платежа
		/// </summary>
		[JsonPropertyName("captured_at")]
		public DateTime? CapturedAt { get; set; }

		/// <summary>
		/// Признак тестового платежа
		/// </summary>
		[JsonPropertyName("test")]
		public bool Test { get; set; }
	}
}
