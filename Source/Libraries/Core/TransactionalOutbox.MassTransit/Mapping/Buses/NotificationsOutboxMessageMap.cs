using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Таблица notifications_outbox_message.
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class NotificationsOutboxMessageMap : OutboxMessageRecordMap<NotificationsOutboxMessage>
	{
		public NotificationsOutboxMessageMap() : base(OutboxBusNames.Notifications) { }
	}
}
