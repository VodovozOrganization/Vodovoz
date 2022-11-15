using System.Text.Json.Serialization;

namespace TrueApi.Dto.Documents
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

	public class Product
	{
		[JsonPropertyName("gtin")]
		public string Gtin { get; set; }

		[JsonPropertyName("gtin_quantity")]
		public string GtinQuantity { get; set; }

		[JsonPropertyName("product_cost")]
		public string ProductCost { get; set; }
	}
}
