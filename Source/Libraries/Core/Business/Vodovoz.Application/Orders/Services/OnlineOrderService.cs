using CustomerNotifications.Contracts.Messages;
using CustomerNotifications.Publisher.Services;
using System;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderService : IOnlineOrderService
	{
		private readonly ICustomerNotificationPublisher _customerNotificationPublisher;

		public OnlineOrderService(ICustomerNotificationPublisher customerNotificationPublisher)
		{
			_customerNotificationPublisher = customerNotificationPublisher ?? throw new ArgumentNullException(nameof(customerNotificationPublisher));
		}

		public void NotifyClientOfOnlineOrderStatusChange(OnlineOrder onlineOrder, CustomerNotificationEventType eventType)
		{
			if(onlineOrder is null)
			{
				return;
			}

			var customerNotificationMessage = new CustomerNotificationMessage()
			{
				OnlineOrderId = onlineOrder.Id,
				CustomerNotificationEventType = eventType,
			};

			_customerNotificationPublisher.PublishAsync(customerNotificationMessage);
		}
	}
}
