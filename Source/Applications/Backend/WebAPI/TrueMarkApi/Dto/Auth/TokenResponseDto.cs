using System.Text.Json.Serialization;

namespace TrueMarkApi.Dto.Auth
{
	/// <summary>
	/// Ответ на запрос авторизационного токена.
	/// </summary>
	public class TokenResponseDto
	{
		/// <summary>
		/// Авторизационный токен в base64-строке
		/// </summary>
		[JsonPropertyName("token")]
		public string Token { get; set; }

		[JsonPropertyName("code")]
		public string Code { get; set; }

		[JsonPropertyName("error_message")]
		public string ErrorMessage { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }
	}
}
