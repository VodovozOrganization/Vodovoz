using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders.Cart;

namespace Vodovoz.Core.Application.Orders.Cart
{
	public class PromoSetCartItem : IPromoSetCartItem
	{
		/// <inheritdoc/>
		public PromotionalSet PromoSet { get; set; }
		/// <inheritdoc/>
		public decimal Count { get; set; }
		/// <inheritdoc/>
		public SaleItemType ItemType => SaleItemType.PromoSet;
	}
}
