using Vodovoz.Core.Data.Orders;

namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Номер измененного онлайн заказа, если есть и заказа
	/// </summary>
	public class ChangedOrderDto
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int? OrderId { get; set; }
		/// <summary>
		/// Номер онлайн заказа
		/// </summary>
		public int? OnlineOrderId { get; set; }

		public static ChangedOrderDto Create(int? orderId, int? onlineOrderId) =>
			new ChangedOrderDto
			{
				OrderId = orderId,
				OnlineOrderId = onlineOrderId
			};
	}
}
