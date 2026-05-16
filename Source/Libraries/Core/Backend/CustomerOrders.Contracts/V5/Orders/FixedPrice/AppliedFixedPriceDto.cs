using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders.OrderItem;

namespace CustomerOrders.Contracts.V5.Orders.FixedPrice
{
	public class AppliedFixedPriceDto
	{
		/// <summary>
		/// Список товаров с фиксой
		/// </summary>
		public IEnumerable<OnlineOrderItemWithFixedPriceV5> OnlineOrderItems { get; set; }
	}
}
