using CustomerOrders.Abstractions.V7.Sale;
using System.Collections.Generic;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.Promotions.Discounts
{
	/// <summary>
	/// Информация по применению скидки по первому заказу
	/// Содержит список товаров с детализацией
	/// </summary>
	public class AppliedFirstOrderDiscountDto
	{
		/// <summary>
		/// Список товаров с детализацией
		/// </summary>
		public IEnumerable<IOrderedCartItemWithDiscountDetails> OnlineOrderItems { get; set; }

		public static AppliedFirstOrderDiscountDto Create(IEnumerable<IOrderedCartItemWithDiscountDetails> onlineOrderItems) =>
			new AppliedFirstOrderDiscountDto
			{
				OnlineOrderItems = onlineOrderItems
			};
	}
}
