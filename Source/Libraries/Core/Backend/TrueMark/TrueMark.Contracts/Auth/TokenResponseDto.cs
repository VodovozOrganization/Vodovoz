using System;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Auth
{
	/// <summary>
	/// Ответ на запрос авторизационного токена.
	/// </summary>
	public class TokenResponseDto
	{
		[JsonPropertyName("uuidToken")]
		public string UuidToken { get; set; }

		[JsonPropertyName("code")]
		public string Code { get; set; }

		[JsonPropertyName("error_message")]
		public string ErrorMessage { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("expireDate")]
		public DateTime ExpireDate { get; set; }
	}
}
