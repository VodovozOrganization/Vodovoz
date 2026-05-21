using System.Collections.Generic;
using CustomerOrders.Contracts.Interfaces;

namespace CustomerOrders.Contracts.V4.Orders.FixedPrice
{
	public class AppliedFixedPriceDto
	{
		/// <summary>
		/// Список товаров с фиксой
		/// </summary>
		public IEnumerable<IOnlineOrderedProductWithFixedPriceV4> OnlineOrderItems { get; set; }
	}
}
