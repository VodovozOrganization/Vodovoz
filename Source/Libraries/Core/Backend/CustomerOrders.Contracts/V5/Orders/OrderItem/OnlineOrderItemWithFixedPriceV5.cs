using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders.Discounts;

namespace CustomerOrders.Contracts.V5.Orders.OrderItem
{
	/// <summary>
	/// Позиция онлайн заказа с фиксой
	/// </summary>
	public class OnlineOrderItemWithFixedPriceV5
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int NomenclatureId { get; set; }
		/// <summary>
		/// Старая цена
		/// </summary>
		public decimal OldPrice { get; set; }
		/// <summary>
		/// Новая цена(фикса)
		/// </summary>
		public decimal? NewPrice { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; set; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		public int? PromoSetId { get; set; }
		/// <summary>
		/// Скидки
		/// </summary>
		public IEnumerable<DiscountDto> Discounts { get; set; }
	}
}
