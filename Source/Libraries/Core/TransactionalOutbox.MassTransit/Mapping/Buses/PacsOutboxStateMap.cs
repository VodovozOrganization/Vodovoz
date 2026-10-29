using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Таблица pacs_outbox_state.
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class PacsOutboxStateMap : OutboxStateRecordMap<PacsOutboxState>
	{
		public PacsOutboxStateMap() : base(OutboxBusNames.Pacs) { }
	}
}
