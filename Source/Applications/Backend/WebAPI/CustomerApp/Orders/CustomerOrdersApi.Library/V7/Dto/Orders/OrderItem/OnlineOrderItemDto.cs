using CustomerOrders.Abstractions.V7.Carts;
using CustomerOrders.Contracts.V7.Orders.Items;
using System.Collections.Generic;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.OrderItem
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
