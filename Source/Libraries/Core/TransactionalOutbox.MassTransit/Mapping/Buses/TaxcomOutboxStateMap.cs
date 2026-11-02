using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Таблица taxcom_outbox_state.
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class TaxcomOutboxStateMap : OutboxStateRecordMap<TaxcomOutboxState>
	{
		public TaxcomOutboxStateMap() : base(OutboxBusNames.Taxcom) { }
	}
}
