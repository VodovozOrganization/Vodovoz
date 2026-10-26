using CustomerApp.Contracts.Sale;

namespace CustomerOrders.Contracts.V8.Carts
{
	public class CartItemDto : ICartItem
	{
		/// <inheritdoc/>
		public SaleItemType ItemType { get; set; }
		
		/// <inheritdoc/>
		public decimal Count { get; set; }
		
		/// <inheritdoc/>
		public int ErpId { get; set; }

		public static CartItemDto Create(SaleItemType itemType, decimal count, int erpId) =>
			new CartItemDto
			{
				ItemType = itemType,
				Count = count,
				ErpId = erpId,
			};
	}
}
