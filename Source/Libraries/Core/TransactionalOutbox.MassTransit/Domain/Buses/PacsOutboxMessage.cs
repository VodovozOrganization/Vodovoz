namespace TransactionalOutbox.MassTransit.Domain
{
	/// <summary>
	/// Строка таблицы pacs_outbox_message (шина телефонии и ЭДО, <see cref="OutboxBusNames.Pacs"/>).
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class PacsOutboxMessage : OutboxMessageRecord
	{
	}
}
