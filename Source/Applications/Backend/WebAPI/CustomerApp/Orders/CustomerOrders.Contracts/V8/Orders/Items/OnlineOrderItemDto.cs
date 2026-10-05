using CustomerOrders.Abstractions.V8.Carts;
using System.Collections.Generic;

namespace CustomerOrders.Contracts.V8.Orders.Items
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
