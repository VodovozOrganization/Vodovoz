using System.Collections.Generic;
using Vodovoz.Core.Data.Orders.V5;
using VodovozBusiness.Domain.Orders.V5;

namespace VodovozBusiness.Nodes.V5
{
	/// <summary>
	/// Позиция онлайн заказа с фиксой
	/// </summary>
	public class OnlineOrderItemWithFixedPriceV5 :  IOnlineOrderedProductWithFixedPriceV5
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
		/// Скидки, привязанные к товару
		/// </summary>
		public IEnumerable<DiscountData> Discounts { get; set; }
	}
}
