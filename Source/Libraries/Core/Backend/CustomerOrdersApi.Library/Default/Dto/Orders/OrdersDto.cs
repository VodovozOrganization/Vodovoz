using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Data.Orders.Default;

namespace CustomerOrdersApi.Library.Default.Dto.Orders
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
