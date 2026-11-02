using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Таблица taxcom_outbox_message.
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class TaxcomOutboxMessageMap : OutboxMessageRecordMap<TaxcomOutboxMessage>
	{
		public TaxcomOutboxMessageMap() : base(OutboxBusNames.Taxcom) { }
	}
}
