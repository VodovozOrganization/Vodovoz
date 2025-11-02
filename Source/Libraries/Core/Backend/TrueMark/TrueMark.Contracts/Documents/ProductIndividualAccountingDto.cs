using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	/// <summary>
	/// Товар, подлежащий индивидуальному учету, входящий в документ для отправки в ЧЗ
	/// </summary>
	public class ProductIndividualAccountingDto
	{
		/// <summary>
		/// Код ЧЗ
		/// </summary>
		[JsonPropertyName("cis")]
		public string TrueMarkCode { get; set; }

		/// <summary>
		/// Стоимость единицы товара
		/// </summary>
		[JsonPropertyName("product_cost")]
		public decimal ProductCost { get; set; }
	}
}
