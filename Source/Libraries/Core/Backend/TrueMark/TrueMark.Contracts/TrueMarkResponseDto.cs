using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Статус регистрации участника
	/// </summary>
	public class TrueMarkRegistrationResultDto
	{
		/// <summary>
		/// Статус
		/// </summary>
		[JsonPropertyName("registrationStatusString")]
		public string RegistrationStatusString { get; set; }

		/// <summary>
		/// Ошибка
		/// </summary>
		[JsonPropertyName("errorMessage")]
		public string ErrorMessage { get; set; }
	}
}
