using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders.OrderItem;

namespace CustomerOrders.Contracts.V5.Orders.FixedPrices
{
	/// <summary>
	/// Данные для проверки возможности применения фиксы
	/// </summary>
	public class CanApplyOnlineOrderFixedPriceV5
	{
		/// <summary>
		/// Id клиента
		/// </summary>
		public int? CounterpartyId { get; set; }
		/// <summary>
		/// Id точки доставки
		/// </summary>
		public int? DeliveryPointId { get; set; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> OnlineOrderItems { get; set; }
	}
}
