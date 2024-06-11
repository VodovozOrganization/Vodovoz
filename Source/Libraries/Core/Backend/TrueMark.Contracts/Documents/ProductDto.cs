using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	public class ProductDto
	{
		[JsonPropertyName("gtin")]
		public string Gtin { get; set; }

		[JsonPropertyName("gtin_quantity")]
		public string GtinQuantity { get; set; }

		[JsonPropertyName("product_cost")]
		public string ProductCost { get; set; }
	}
}
