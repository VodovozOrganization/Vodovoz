using System.Text.Json.Serialization;

namespace YandexPayApi.Library.Responses
{
	/// <summary>
	/// Базовый ответ от API Яндекс Пэй 
	/// </summary>
	/// <typeparam name="T">Тип данных в поле data</typeparam>
	public class YandexPayApiResponse<T>
	{
		/// <summary>
		/// Код ответа
		/// </summary>
		[JsonPropertyName("code")]
		public int? Code { get; set; }

		/// <summary>
		/// Статус ответа
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Данные ответа
		/// </summary>
		[JsonPropertyName("data")]
		public T Data { get; set; }

		/// <summary>
		/// Причина ошибки (при неудаче)
		/// </summary>
		[JsonPropertyName("reason")]
		public string Reason { get; set; }

		/// <summary>
		/// Код причины ошибки (при неудаче)
		/// </summary>
		[JsonPropertyName("reasonCode")]
		public string ReasonCode { get; set; }

		/// <summary>
		/// Детали ошибки (при неудаче)
		/// </summary>
		[JsonPropertyName("details")]
		public object Details { get; set; }
	}
}
