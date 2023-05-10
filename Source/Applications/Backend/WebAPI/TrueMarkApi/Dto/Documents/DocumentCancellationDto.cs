using System.Text.Json.Serialization;

namespace TrueMarkApi.Dto.Documents
{
	public class DocumentCancellationDto
	{
		[JsonPropertyName("inn")]
		public string Inn { get; set; }

		[JsonPropertyName("lk_gtin_receipt_id")]
		public string LkGtinReceiptId { get; set; }

		[JsonPropertyName("version")]
		public int Version { get; set; }
	}
}
