using System.Text.Json.Serialization;

namespace TrueApi.Dto.Auth
{
	/// <summary>
	/// Данные для запроса авторизационного токена.
	/// </summary>
	public class TokenRequestDto
	{
		/// <summary>
		/// uuid - уникальный идентификатор подписанных данных
		/// </summary>
		[JsonPropertyName("uuid")]
		public string Uuid { get; set; }

		/// <summary>
		/// подписанные УКЭП зарегистрированного участника случайные данные в base64
		/// </summary>
		[JsonPropertyName("data")]
		public string Data { get; set; }
	}
}
