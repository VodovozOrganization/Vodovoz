namespace CustomerOrdersApi.Library.V6.Dto.Orders
{
	/// <summary>
	/// Постраничное представление активных заказов клиента
	/// </summary>
	public class ActiveOrdersDto
	{
		/// <summary>
		/// Количество активных заказов клиента
		/// </summary>
		public int OrdersCount { get; set; }

		/// <summary>
		/// Активные заказы
		/// </summary>
		public ActiveOrderDto[] Orders { get; set; }
	}
}
