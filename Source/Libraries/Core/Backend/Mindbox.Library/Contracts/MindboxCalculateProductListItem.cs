using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Позиция списка товаров для расчёта
	/// </summary>
	public sealed class MindboxCalculateProductListItem
	{
		/// <summary>
		/// Базовая цена за 1 шт.
		/// </summary>
		[JsonPropertyName("basePricePerItem")]
		public decimal BasePricePerItem { get; set; }

		/// <summary>
		/// Товар
		/// </summary>
		[JsonPropertyName("product")]
		public MindboxProduct Product { get; set; }
	}
}
