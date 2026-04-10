using TransactionalOutbox.Contracts;

namespace CustomerPushNotifications.Contracts
{
	public class CustomerNotificationDomainEvent: IOutboxDomainEvent
	{
		/// <summary>
		/// Код онлайн-заказа в ERP
		/// </summary>
		public int OnlineOrderId { get; }

		/// <summary>
		/// Тип события для уведомления
		/// </summary>
		public CustomerNotificationEventType CustomerNotificationEventType { get; }

		public CustomerNotificationDomainEvent(int onlineOrderId, CustomerNotificationEventType customerNotificationEventType)
		{
			OnlineOrderId = onlineOrderId;
			CustomerNotificationEventType = customerNotificationEventType;
		}
		
		public string GetDeduplicationKey() => $"{nameof(CustomerNotificationEventType)}:{nameof(OnlineOrderId)}={OnlineOrderId}:{nameof(CustomerNotificationEventType)}={CustomerNotificationEventType}";

		public int GetAggregateId() => OnlineOrderId;
	}
}
