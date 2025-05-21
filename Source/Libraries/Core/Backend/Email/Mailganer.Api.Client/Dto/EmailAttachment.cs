using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	public class EmailAttachment
	{
		[JsonPropertyName("attach_files.name")]
		public string Filename { get; set; }

		[JsonPropertyName("attach_files.filebody")]
		public string Base64Content { get; set; }
	}
}
