using System.Text.Json.Serialization;

namespace VodovozInfrastructure.Endpoints
{
	internal class ErrorMessage
	{
		[JsonPropertyName("error")]
		public string Error { get; set; }
	}
}
