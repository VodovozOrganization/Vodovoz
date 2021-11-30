using System;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Domain
{
	public class OrderDailyNumberController : IOrderDailyNumberController
	{
		private readonly IOrderRepository _orderRepository;

		public OrderDailyNumberController(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}
		
		public void UpdateDailyNumber(Order order)
		{
			if(!order.DeliveryDate.HasValue)
			{
				throw new InvalidOperationException("Delivery date can't be null");
			}
			
			if(order.OrderStatus == OrderStatus.NewOrder || order.OrderStatus == OrderStatus.WaitForPayment)
			{
				order.DailyNumber = null;
			}
			else if(order.DailyNumber == null
				|| order.DeliveryDate != _orderRepository.GetOrderDeliveryDate(order.UoW, order.Id))
			{
				order.DailyNumber = _orderRepository.GetMaxOrderDailyNumberForDate(order.UoW, order.DeliveryDate.Value) ?? 0;
				order.DailyNumber++;
			}
		}
	}
}
