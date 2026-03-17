using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa
{
	/// <summary>
	/// Модель ошибки API ЮKassa
	/// </summary>
	public class YooKassaErrorResponse
	{
		/// <summary>
		/// Тип ошибки
		/// </summary>
		[JsonPropertyName("type")]
		public string Type { get; set; }

		/// <summary>
		/// Идентификатор ошибки
		/// </summary>
		[JsonPropertyName("id")]
		public string Id { get; set; }

		/// <summary>
		/// Код ошибки
		/// </summary>
		[JsonPropertyName("code")]
		public string Code { get; set; }

		/// <summary>
		/// Описание ошибки
		/// </summary>
		[JsonPropertyName("description")]
		public string Description { get; set; }

		/// <summary>
		/// Параметр, вызвавший ошибку
		/// </summary>
		[JsonPropertyName("parameter")]
		public string Parameter { get; set; }

		/// <summary>
		/// Время в секундах, через которое можно повторить запрос
		/// </summary>
		[JsonPropertyName("retry_after")]
		public int? RetryAfter { get; set; }
	}
}
