using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Таблица online_orders_outbox_message.
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class OnlineOrdersOutboxMessageMap : OutboxMessageRecordMap<OnlineOrdersOutboxMessage>
	{
		public OnlineOrdersOutboxMessageMap() : base(OutboxBusNames.OnlineOrders) { }
	}
}
