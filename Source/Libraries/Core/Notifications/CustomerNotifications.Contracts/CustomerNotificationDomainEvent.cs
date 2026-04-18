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
		/// Код нового перенесенного заказа
		/// </summary>
		public int? RescheduledNewOrderId { get; }

		/// <summary>
		/// Сообщение клиенту недовезённого заказа
		/// </summary>
		public string UndeliveryCustomerMessage { get; }

		/// <summary>
		/// Тип события для уведомления
		/// </summary>
		public CustomerNotificationEventType CustomerNotificationEventType { get; }

		/// <summary>
		/// Внешний источник
		/// </summary>
		public Source EventSource{ get; }

		public CustomerNotificationDomainEvent(
			CustomerNotificationEventType customerNotificationEventType,
			Source? source = null,
			int? onlineOrderId = null,
			int? orderId = null,
			int? rescheduledOrderId = null,
			string undeliveryCustomerMessage = null)
		{
			if(onlineOrderId == null && orderId == null)
			{
				throw new ArgumentException($"Для события {customerNotificationEventType} должен быть заполнен хотя бы один из идентификаторов заказа: {nameof(OnlineOrderId)} или {nameof(OrderId)}.");
			}

			OnlineOrderId = onlineOrderId;
			OrderId = orderId;
			RescheduledNewOrderId = rescheduledOrderId;
			UndeliveryCustomerMessage = undeliveryCustomerMessage;
			CustomerNotificationEventType = customerNotificationEventType;
			EventSource = source ?? Source.MobileApp;
		}

		public string GetDeduplicationKey() => $"" +
			$"Event={nameof(CustomerNotificationDomainEvent)}" +
			$"AggregateId={GetAggregateId()}" +
			$":{nameof(CustomerNotificationEventType)}={CustomerNotificationEventType}";

		public int GetAggregateId() => OnlineOrderId ?? OrderId ?? throw new ArgumentNullException();
	}
}
