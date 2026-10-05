using CustomerOrders.Abstractions.V8.Carts;
using CustomerOrders.Contracts.V8.Orders.Items;
using System.Collections.Generic;

namespace CustomerOrdersApi.Library.V8.Dto.Orders.OrderItem
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
