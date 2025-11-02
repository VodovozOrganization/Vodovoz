using System.Text.Json.Serialization;

namespace VodovozInfrastructure.Endpoints
{
	public class UserData
	{
		[JsonPropertyName("username")]
		public string Username { get; set; }
		[JsonPropertyName("password")]
		public string Password { get; set; }
		[JsonPropertyName("userRole")]
		public string UserRole { get; set; }
	}
}
