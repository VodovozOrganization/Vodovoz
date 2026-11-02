namespace TransactionalOutbox.MassTransit.Domain
{
	/// <summary>
	/// Строка таблицы email_outbox_message (шина почты, <see cref="OutboxBusNames.Email"/>).
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class EmailOutboxMessage : OutboxMessageRecord
	{
	}
}
