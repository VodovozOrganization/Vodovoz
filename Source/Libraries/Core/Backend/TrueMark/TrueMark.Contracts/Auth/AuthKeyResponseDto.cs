using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Auth
{
	/// <summary>
	/// Ответ на запрос для авторизации.
	/// </summary>
	public class AuthKeyResponseDto
	{
		/// <summary>
		/// уникальный идентификатор сгенерированных случайных данных, тип string
		/// </summary>
		[JsonPropertyName("uuid")]
		public string Uuid { get; set; }

		/// <summary>
		/// случайная строка, тип string
		/// </summary>
		[JsonPropertyName("data")]
		public string Data { get; set; }
	}
}
