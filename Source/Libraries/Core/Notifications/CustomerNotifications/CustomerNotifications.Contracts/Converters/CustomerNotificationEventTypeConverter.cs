using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Contracts.Converters
{
	public static class CustomerNotificationEventTypeConverter
	{
		public static CustomerNotificationEventType ToCustomerNotificationEventType(this ExternalOrderStatus status)
		{
			// Ждём от Константина информацию по соответствию статусов и типов событий

			switch(status)
			{
				case ExternalOrderStatus.OrderProcessing:
					return CustomerNotificationEventType.OrderProcessing;
				case ExternalOrderStatus.OrderPerformed:
					return CustomerNotificationEventType.OrderPerformed;
				case ExternalOrderStatus.WaitingForPayment:
					return CustomerNotificationEventType.OrderAwaitingPayment;
				case ExternalOrderStatus.OrderCollecting:
					return CustomerNotificationEventType.CourierAssigned;
				case ExternalOrderStatus.OrderDelivering:
					return CustomerNotificationEventType.OrderDelivering;
				case ExternalOrderStatus.OrderCompleted:
					return CustomerNotificationEventType.DeliveryCompleted;
				case ExternalOrderStatus.Canceled:
					return CustomerNotificationEventType.OrderCanceled;
				default:
					return CustomerNotificationEventType.OrderProcessing;
			}
		}
	}
}
