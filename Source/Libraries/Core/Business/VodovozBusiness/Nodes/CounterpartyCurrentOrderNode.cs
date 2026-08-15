using System;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	public class CounterpartyCurrentOrderNode
	{
		private CounterpartyCurrentOrderNode(int orderId, DateTime? deliveryDate, OrderStatus status)
		{
			OrderId = orderId;
			DeliveryDate = deliveryDate;
			OrderStatus = status;
		}
		
		/// <summary>
		/// Идентификатор заказа
		/// </summary>
		public int OrderId { get; set; }
		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
		/// <summary>
		/// Статус заказа
		/// </summary>
		public OrderStatus OrderStatus { get; set; }
		public static CounterpartyCurrentOrderNode Create(int orderId, DateTime? deliveryDate, OrderStatus status) =>
			new CounterpartyCurrentOrderNode(orderId, deliveryDate, status);
	}
}
