using System.Text.Json.Serialization;

namespace YandexPayApi.Library.Models
{
	/// <summary>
	/// Типы операций в Яндекс Пэй
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum YandexPayOperationType
	{
		/// <summary>
		/// Авторизация платежа
		/// </summary>
		[JsonPropertyName("AUTHORIZE")]
		Authorize,

		/// <summary>
		/// Возврат средств
		/// </summary>
		[JsonPropertyName("REFUND")]
		Refund
	}
}
