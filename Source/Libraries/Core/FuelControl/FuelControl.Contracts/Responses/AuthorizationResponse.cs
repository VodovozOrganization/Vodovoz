using FuelControl.Contracts.Dto;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Responses
{
	/// <summary>
	/// Ответ сервера на запрос авторизации
	/// </summary>
	public class AuthorizationResponse : ResponseBase
	{
		/// <summary>
		/// Данные пользователя
		/// </summary>
		[JsonPropertyName("data")]
		public UserDataDto UserData { get; set; }
	}
}
