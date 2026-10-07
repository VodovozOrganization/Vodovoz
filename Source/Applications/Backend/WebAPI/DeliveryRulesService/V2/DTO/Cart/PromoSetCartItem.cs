using CustomerApp.Contracts.Sale;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders.Cart;

namespace DeliveryRulesService.V2.DTO.Cart
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
