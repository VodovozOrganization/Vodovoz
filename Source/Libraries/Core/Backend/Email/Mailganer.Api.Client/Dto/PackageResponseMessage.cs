using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	public class PackageResponseMessage
	{
		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("pack_id")]
		public int PackId { get; set; }
	}
}
