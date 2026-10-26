using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Таблица email_outbox_message.
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class EmailOutboxMessageMap : OutboxMessageRecordMap<EmailOutboxMessage>
	{
		public EmailOutboxMessageMap() : base(OutboxBusNames.Email) { }
	}
}
