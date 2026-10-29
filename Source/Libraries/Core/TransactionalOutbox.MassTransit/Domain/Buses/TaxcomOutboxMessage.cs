namespace TransactionalOutbox.MassTransit.Domain
{
	/// <summary>
	/// Строка таблицы taxcom_outbox_message (шина документов Такском, <see cref="OutboxBusNames.Taxcom"/>).
	/// После перехода на один vhost RabbitMQ остаётся одна пара таблиц outbox, классы остальных шин удаляются
	/// </summary>
	public class TaxcomOutboxMessage : OutboxMessageRecord
	{
	}
}
