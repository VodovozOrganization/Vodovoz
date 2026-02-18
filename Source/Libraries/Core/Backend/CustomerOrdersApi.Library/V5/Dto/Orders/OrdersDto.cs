using Vodovoz.Core.Data.Orders.V5;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
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
