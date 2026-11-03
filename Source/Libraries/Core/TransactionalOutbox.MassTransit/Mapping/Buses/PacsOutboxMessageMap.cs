using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Таблица pacs_outbox_message.
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class PacsOutboxMessageMap : OutboxMessageRecordMap<PacsOutboxMessage>
	{
		public PacsOutboxMessageMap() : base(OutboxBusNames.Pacs) { }
	}
}
