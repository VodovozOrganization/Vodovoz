using System.Text.Json.Serialization;

namespace TrueMarkApi.Library.Dto
{
	public class TrueMarkResponseResultDto
	{
		[JsonPropertyName("registrationStatusString")]
		public string RegistrationStatusString { get; set; }
		[JsonPropertyName("errorMessage")]
		public string ErrorMessage { get; set; }
	}
}
