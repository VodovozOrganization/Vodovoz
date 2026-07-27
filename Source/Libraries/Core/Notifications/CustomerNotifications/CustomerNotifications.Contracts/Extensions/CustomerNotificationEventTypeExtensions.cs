using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Domain.Orders;

namespace CustomerNotifications.Contracts.Extensions
{
	public static class CustomerNotificationEventTypeExtensions
	{
		public static CustomerNotificationEventType? ToCustomerNotificationEventType(this OrderStatus orderStatus)
		{
			switch(orderStatus)
			{
				case OrderStatus.OnTheWay:
					return CustomerNotificationEventType.CourierAssigned;
				case OrderStatus.Shipped:
					return CustomerNotificationEventType.DeliveryCompleted;
				default:
					return null;
			}
		}
	}
}
