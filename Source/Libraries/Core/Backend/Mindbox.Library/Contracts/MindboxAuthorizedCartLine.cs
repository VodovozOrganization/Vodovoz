using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Позиция заказа в запросе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartLine
	{
		/// <summary>
		/// Порядковый номер позиции
		/// </summary>
		[JsonPropertyName("lineNumber")]
		public int LineNumber { get; set; }

		/// <summary>
		/// Товар
		/// </summary>
		[JsonPropertyName("product")]
		public MindboxProduct Product { get; set; }

		/// <summary>
		/// Количество
		/// </summary>
		[JsonPropertyName("quantity")]
		public decimal Quantity { get; set; }

		/// <summary>
		/// Тип количества
		/// </summary>
		[JsonPropertyName("quantityType")]
		public string QuantityType { get; set; }

		/// <summary>
		/// Цена за единицу после скидок ERP
		/// </summary>
		[JsonPropertyName("basePricePerItem")]
		public decimal BasePricePerItem { get; set; }

		/// <summary>
		/// Статус позиции
		/// </summary>
		[JsonPropertyName("status")]
		public MindboxAuthorizedCartLineStatus Status { get; set; }
	}
}
