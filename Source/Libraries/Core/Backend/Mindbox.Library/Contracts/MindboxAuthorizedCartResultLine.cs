using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Позиция заказа в ответе CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartResultLine
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
		/// Цена за единицу после скидок ERP
		/// </summary>
		[JsonPropertyName("basePricePerItem")]
		public decimal BasePricePerItem { get; set; }

		/// <summary>
		/// Стоимость позиции со скидками
		/// </summary>
		[JsonPropertyName("discountedPriceOfLine")]
		public decimal? DiscountedPriceOfLine { get; set; }

		/// <summary>
		/// Применённые к позиции акции
		/// </summary>
		[JsonPropertyName("appliedPromotions")]
		public List<MindboxAppliedPromotion> AppliedPromotions { get; set; }
	}
}
