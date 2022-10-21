using System.Text.Json.Serialization;

namespace EdoWebApi.Dto
{
	public class TrueApiRegistrationDto
	{
		[JsonPropertyName("is_registered")]
		public bool IsRegistered { get; set; }
	}
}
