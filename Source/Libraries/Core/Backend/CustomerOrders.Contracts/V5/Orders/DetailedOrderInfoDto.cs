using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders.PromoSets;

namespace CustomerOrders.Contracts.V5.Orders
{
	/// <summary>
	/// Детальная информация о заказе
	/// </summary>
	public class DetailedOrderInfoDto : OrderDto
	{
		/// <summary>
		/// Значение таймера для оплаты заказа
		/// </summary>
		public int? TimerForPaySeconds { get; set; }
		
		/// <summary>
		/// Доступность повторения заказа
		/// </summary>
		public bool AvailableRepeatOrder { get; set; }
		
		/// <summary>
		/// Быстрая доставка
		/// </summary>
		public bool IsFastDelivery { get; set; }

		/// <summary>
		/// Источник онлайн оплаты
		/// </summary>
		public ExternalPaymentSource? OnlinePaymentSource { get; set; }
		
		/// <summary>
		/// Тип онлайн оплаты
		/// </summary>
		public ExternalOrderPaymentType? OnlinePaymentType { get; set; }
		
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
		public IEnumerable<OrderPromoSetDto> PromoSets { get; set; }
	}
}
