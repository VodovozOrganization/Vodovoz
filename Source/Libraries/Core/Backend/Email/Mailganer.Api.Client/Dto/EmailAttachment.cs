using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	public class EmailAttachment
	{
		[JsonPropertyName("name")]
		public string Filename { get; set; }

		[JsonPropertyName("filebody")]
		public string Base64Content { get; set; }
	}
}
