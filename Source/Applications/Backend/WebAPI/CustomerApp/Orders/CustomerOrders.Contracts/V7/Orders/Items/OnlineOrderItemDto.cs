using CustomerOrders.Abstractions.V7.Carts;
using System.Collections.Generic;

namespace CustomerOrders.Contracts.V7.Orders.Items
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OnlineOrderItemDto : OnlineOrderItemBaseDto, IOrderedCartItem
	{
		/// <inheritdoc/>
		public IEnumerable<int> DiscountIds { get; set; }
	}
}
