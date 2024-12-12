using Vodovoz.Core.Data.Orders;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// Постраничное представление заказов клиента
	/// </summary>
	public class OrdersDto
	{
		/// <summary>
		/// Количество страниц с заказами
		/// </summary>
		public int OrdersCount { get; set; }
		
		/// <summary>
		/// Заказы
		/// </summary>
		public OrderDto[] Orders { get; set; }
	}
}
