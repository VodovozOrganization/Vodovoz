namespace TransactionalOutbox.MassTransit.Domain
{
	/// <summary>
	/// Строка таблицы notifications_outbox_message (шина уведомлений, <see cref="OutboxBusNames.Notifications"/>).
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class NotificationsOutboxMessage : OutboxMessageRecord
	{
	}
}
