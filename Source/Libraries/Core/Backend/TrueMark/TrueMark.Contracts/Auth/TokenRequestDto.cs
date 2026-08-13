using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Auth
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

		/// <summary>
		/// ИНН
		/// </summary>
		[JsonPropertyName("inn")]
		public string Inn { get; set; }

		/// <summary>
		/// Реквизиты действующего аттестата соответствия объекта информатизации, 
		/// выданного органом по аттестации объектов информатизации
		/// </summary>
		[JsonPropertyName("details")]
		public string Details { get; set; }

		/// <summary>
		/// Признак запроса единого токена в виде uuid
		/// </summary>
		[JsonPropertyName("unitedToken")]
		public bool UnitedToken { get; set; }
	}
}
