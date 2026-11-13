using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Тело запроса операции CalculateProductList
	/// </summary>
	public sealed class MindboxCalculateProductListRequest
	{
		/// <summary>
		/// Клиент, для которого рассчитывается список товаров
		/// </summary>
		[JsonPropertyName("customer")]
		public MindboxCalculateProductListCustomer Customer { get; set; }

		/// <summary>
		/// Список товаров для расчёта
		/// </summary>
		[JsonPropertyName("productList")]
		public MindboxCalculateProductList ProductList { get; set; }
	}
}
