using System.Collections.Generic;
using Vodovoz.Core.Data.Sale;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Dto.Orders
{
	/// <summary>
	/// Детальная информация о заказе
	/// </summary>
	public class DetailedOrderInfoDto : ActiveOrderDto
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
		/// Доступность переноса даты/времени доставки
		/// </summary>
		public bool AvailableChangeDeliverySchedule { get; set; }

		/// <summary>
		/// Доступность отмены заказа
		/// </summary>
		public bool AvailableCancelOrder { get; set; }

		/// <summary>
		/// Быстрая доставка
		/// </summary>
		public bool IsFastDelivery { get; set; }

		/// <summary>
		/// Источник онлайн оплаты
		/// </summary>
		public OnlinePaymentSource? OnlinePaymentSource { get; set; }
		
		/// <summary>
		/// Тип онлайн оплаты
		/// </summary>
		public OnlineOrderPaymentType? OnlinePaymentType { get; set; }
		
		/// <summary>
		/// Причины оценки
		/// </summary>
		public IEnumerable<int> RatingReasonsIds { get; set; }
		
		/// <summary>
		/// Комментарий к оценке
		/// </summary>
		public string OrderRatingComment { get; set; }

		/// <summary>
		/// Номер телефона водителя в Mango
		/// </summary>
		public string DriversMangoNumber { get; set; }

		/// <summary>
		/// Товары/услуги заказа
		/// </summary>
		public IList<OnlineOrderItemWithDiscountDetailsDto> OrderItems { get; set; }
	}
}
