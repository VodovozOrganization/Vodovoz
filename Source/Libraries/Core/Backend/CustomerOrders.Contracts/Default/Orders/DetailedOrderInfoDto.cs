using System.Collections.Generic;

namespace CustomerOrders.Contracts.Default.Orders
{
	/// <summary>
	/// Детальная информация о заказе
	/// </summary>
	public class DetailedOrderInfoDto : OrderDto
	{
		/// <summary>
		/// Быстрая доставка
		/// </summary>
		public bool IsFastDelivery { get; set; }
		
		/// <summary>
		/// Причины оценки
		/// </summary>
		public IEnumerable<int> RatingReasonsIds { get; set; }
		
		/// <summary>
		/// Комментарий к оценке
		/// </summary>
		public string OrderRatingComment { get; set; }
		
		/// <summary>
		/// Товары без промонаборов
		/// </summary>
		public IEnumerable<OrderItemDto> OrderItems { get; set; }
		
		/// <summary>
		/// Промонаборы
		/// </summary>
		public IEnumerable<PromoSetDto> PromoSets { get; set; }
	}
}
