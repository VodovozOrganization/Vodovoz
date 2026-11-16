using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Ответ операции CalculateProductList
	/// </summary>
	public sealed class MindboxCalculateProductListResponse
	{
		/// <summary>
		/// Статус операции
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Результат расчёта списка товаров
		/// </summary>
		[JsonPropertyName("productList")]
		public MindboxCalculateProductListResult ProductList { get; set; }
	}
}
