using System;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Предстоящий заказ
	/// </summary>
	public class TemplateUpcomingOrderDto
	{
		/// <summary>
		/// Идентификатор заказа
		/// </summary>
		public int? OrderId { get; set; }

		/// <summary>
		/// Идентификатор онлайн заказа
		/// </summary>
		public int OnlineOrderId { get; set; }

		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime DeliveryDate { get; set; }

		public static TemplateUpcomingOrderDto Create(int onlineOrderId, DateTime deliveryDate, int? orderId = null)
		{
			return new TemplateUpcomingOrderDto
			{
				OnlineOrderId = onlineOrderId,
				DeliveryDate = deliveryDate,
				OrderId = orderId
			};
		}
	}
}
