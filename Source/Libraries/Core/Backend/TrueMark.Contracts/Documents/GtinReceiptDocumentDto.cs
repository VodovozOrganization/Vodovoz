using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	public class GtinReceiptDocumentDto
	{
		[JsonPropertyName("document_format")]
		public string DocumentFormat { get; set; }

		[JsonPropertyName("product_document")]
		public string ProductDocument { get; set; }

		[JsonPropertyName("type")]
		public string Type { get; set; }

		[JsonPropertyName("signature")]
		public string Signature { get; set; }
	}
}
