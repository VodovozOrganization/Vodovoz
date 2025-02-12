using System;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Domain
{
	public class OrderDailyNumberController : IOrderDailyNumberController
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IUnitOfWorkFactory _uowFactory;

		public OrderDailyNumberController(IOrderRepository orderRepository, IUnitOfWorkFactory uowFactory)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
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
				|| order.DeliveryDate != _orderRepository.GetOrderDeliveryDate(_uowFactory, order.Id))
			{
				order.DailyNumber = _orderRepository.GetMaxOrderDailyNumberForDate(_uowFactory, order.DeliveryDate.Value) ?? 0;
				order.DailyNumber++;
			}
		}
	}
}
