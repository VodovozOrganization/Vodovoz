using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay
{
	/// <summary>
	/// Статусы операций в Яндекс Пэй
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum YandexPayOperationStatus
	{
		/// <summary>
		/// Операция создана, ожидает выполнения
		/// </summary>
		[JsonPropertyName("PENDING")]
		Pending,

		/// <summary>
		/// Операция выполняется
		/// </summary>
		[JsonPropertyName("PROCESSING")]
		Processing,

		/// <summary>
		/// Операция успешно завершена
		/// </summary>
		[JsonPropertyName("SUCCESS")]
		Success,

		/// <summary>
		/// Операция завершилась ошибкой
		/// </summary>
		[JsonPropertyName("FAIL")]
		Fail,

		/// <summary>
		/// Операция отменена
		/// </summary>
		[JsonPropertyName("CANCELED")]
		Canceled
	}
}
