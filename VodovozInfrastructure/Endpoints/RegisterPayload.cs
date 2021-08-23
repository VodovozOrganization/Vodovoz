using System.Text.Json.Serialization;

namespace VodovozInfrastructure.Endpoints
{
	public class RegisterPayload
	{
		[JsonPropertyName("username")]
		public string Username { get; set; }
		[JsonPropertyName("password")]
		public string Password { get; set; }
	}
}
