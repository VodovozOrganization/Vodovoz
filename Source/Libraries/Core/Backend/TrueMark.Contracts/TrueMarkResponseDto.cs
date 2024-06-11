using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	public class TrueMarkRegistrationResultDto
	{
		[JsonPropertyName("registrationStatusString")]
		public string RegistrationStatusString { get; set; }

		[JsonPropertyName("errorMessage")]
		public string ErrorMessage { get; set; }
	}
}
