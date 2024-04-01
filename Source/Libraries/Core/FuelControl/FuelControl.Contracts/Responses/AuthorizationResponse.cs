using FuelControl.Contracts.Dto;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Responses
{
	/// <summary>
	/// Ответ сервера на запрос авторизации
	/// </summary>
	public class AuthorizationResponse
	{
		/// <summary>
		/// Статус выполнения запроса
		/// </summary>
		[JsonPropertyName("status")]
		public RequestStatus RequestStatus { get; set; }

		/// <summary>
		/// Данные пользователя
		/// </summary>
		[JsonPropertyName("data")]
		public UserData UserData { get; set; }

		/// <summary>
		/// Время ответа
		/// </summary>
		[JsonPropertyName("timestamp")]
		public int? Timestamp { get; set; }
	}
}
