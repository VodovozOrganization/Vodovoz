using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Таблица email_outbox_state.
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class EmailOutboxStateMap : OutboxStateRecordMap<EmailOutboxState>
	{
		public EmailOutboxStateMap() : base(OutboxBusNames.Email) { }
	}
}
