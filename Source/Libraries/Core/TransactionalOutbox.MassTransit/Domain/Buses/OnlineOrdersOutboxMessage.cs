namespace TransactionalOutbox.MassTransit.Domain
{
	/// <summary>
	/// Строка таблицы online_orders_outbox_message (шина онлайн-заказов, <see cref="OutboxBusNames.OnlineOrders"/>).
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class OnlineOrdersOutboxMessage : OutboxMessageRecord
	{
	}
}
