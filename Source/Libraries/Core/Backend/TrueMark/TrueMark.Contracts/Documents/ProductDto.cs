using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	/// <summary>
	/// Номенклатура дял списания
	/// </summary>
	public class ProductDto
	{
		/// <summary>
		/// Номер товарной продукции GTIN
		/// </summary>
		[JsonPropertyName("gtin")]
		public string Gtin { get; set; }

		/// <summary>
		/// Кол-во
		/// </summary>
		[JsonPropertyName("gtin_quantity")]
		public string GtinQuantity { get; set; }

		/// <summary>
		/// Стоимость в копейках
		/// </summary>
		[JsonPropertyName("product_cost")]
		public string ProductCost { get; set; }
	}
}
