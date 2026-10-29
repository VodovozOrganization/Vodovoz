using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Таблица online_orders_outbox_state.
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class OnlineOrdersOutboxStateMap : OutboxStateRecordMap<OnlineOrdersOutboxState>
	{
		public OnlineOrdersOutboxStateMap() : base(OutboxBusNames.OnlineOrders) { }
	}
}
