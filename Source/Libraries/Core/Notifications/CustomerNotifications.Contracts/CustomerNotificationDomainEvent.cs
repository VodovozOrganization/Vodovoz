using System;
using TransactionalOutbox.Contracts;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Contracts
{
	/// <summary>
	/// Событие для уведомлений клиентов. Содержит информацию о том, какое событие произошло, и какую информацию нужно передать для формирования уведомления.
	/// </summary>
	public class CustomerNotificationDomainEvent : IOutboxDomainEvent
	{
		/// <summary>
		/// Код онлайн-заказа в ERP
		/// </summary>
		public int? OnlineOrderId { get; }

		/// <summary>
		/// Код заказа
		/// </summary>
		public int? OrderId { get; }

		/// <summary>
		/// Тип события для уведомления
		/// </summary>
		public CustomerNotificationEventType CustomerNotificationEventType { get; }

		/// <summary>
		/// Внешний источник
		/// </summary>
		public Source EventSource{ get; }

		public CustomerNotificationDomainEvent(CustomerNotificationEventType customerNotificationEventType, Source? source = null, int? onlineOrderId = null, int? orderId = null)
		{
			OnlineOrderId = onlineOrderId;
			OrderId = orderId;
			CustomerNotificationEventType = customerNotificationEventType;
			EventSource = source ?? Source.MobileApp;
		}

		public string GetDeduplicationKey() => $"{nameof(CustomerNotificationEventType)}:{nameof(OnlineOrderId)}={OnlineOrderId}:{nameof(CustomerNotificationEventType)}={CustomerNotificationEventType}";

		public int GetAggregateId() => OnlineOrderId ?? OrderId ?? throw new ArgumentNullException();
	}
}
