using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Таблица notifications_outbox_state.
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class NotificationsOutboxStateMap : OutboxStateRecordMap<NotificationsOutboxState>
	{
		public NotificationsOutboxStateMap() : base(OutboxBusNames.Notifications) { }
	}
}
