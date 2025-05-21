using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	public class PackageSendResponse
	{
		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("message")]
		public PackageResponseMessage Message { get; set; }
	}
}
