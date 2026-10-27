using System.Collections.Generic;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces.Sale;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Позиция онлайн заказа с фиксой
	/// </summary>
	public class OnlineOrderItemWithFixedPriceV7 :  IOnlineOrderedProductWithFixedPriceV7
	{
		/// <inheritdoc/>
		public int ErpId { get; set; }
		/// <inheritdoc/>
		public SaleItemType ItemType { get; set; }
		/// <inheritdoc/>
		public decimal? PriceWithoutDiscount { get; set; }
		/// <inheritdoc/>
		public decimal Price { get; set; }
		/// <inheritdoc/>
		public decimal Count { get; set; }
		/// <inheritdoc/>
		public IEnumerable<int> DiscountIds { get; set; }
	}
}
