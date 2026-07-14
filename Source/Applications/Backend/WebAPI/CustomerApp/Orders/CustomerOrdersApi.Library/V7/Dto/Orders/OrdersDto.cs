using CustomerOrdersApi.Library.V6.Dto.Orders;

namespace CustomerOrdersApi.Library.V7.Dto.Orders
{
	/// <summary>
	/// Постраничное представление заказов клиента
	/// </summary>
	public class OrdersDto
	{
		/// <summary>
		/// Количество заказов клиента
		/// </summary>
		public int OrdersCount { get; set; }
		
		/// <summary>
		/// Заказы
		/// </summary>
		public OrderDto[] Orders { get; set; }
	}
}
