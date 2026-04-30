namespace CustomerOrders.Contracts.V4.Orders
{
	/// <summary>
	/// Номер измененного онлайн заказа, если есть и заказа
	/// </summary>
	public class ChangedOrderDto
	{
		/// <summary>
		/// Номер онлайн заказа
		/// </summary>
		public int? OnlineOrderId { get; set; }

		public static ChangedOrderDto Create(int? onlineOrderId) =>
			new ChangedOrderDto
			{
				OnlineOrderId = onlineOrderId
			};
	}
}
