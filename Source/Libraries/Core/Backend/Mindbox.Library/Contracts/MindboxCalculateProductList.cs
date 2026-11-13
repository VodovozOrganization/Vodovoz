using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Список товаров для расчёта
	/// </summary>
	public sealed class MindboxCalculateProductList
	{
		/// <summary>
		/// Позиции списка товаров
		/// </summary>
		[JsonPropertyName("items")]
		public List<MindboxCalculateProductListItem> Items { get; set; }
	}
}
