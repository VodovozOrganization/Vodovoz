using System;

namespace TransactionalOutbox.MassTransit.Domain
{
	/// <summary>
	/// Создаёт записи outbox нужной шины. Новая шина добавляется сюда, в <see cref="OutboxBusNames"/>,
	/// в маппинги NHibernate (папка Mapping) и в модель БД (таблицы X_outbox_state, X_outbox_message).
	/// После перехода на один vhost RabbitMQ фабрика не нужна: останется одна пара классов записей
	/// </summary>
	public static class OutboxRecordFactory
	{
		public static OutboxStateRecord CreateState(string busName)
		{
			switch(busName)
			{
				case OutboxBusNames.Pacs:
					return new PacsOutboxState();
				case OutboxBusNames.Notifications:
					return new NotificationsOutboxState();
				case OutboxBusNames.Email:
					return new EmailOutboxState();
				case OutboxBusNames.Taxcom:
					return new TaxcomOutboxState();
				case OutboxBusNames.OnlineOrders:
					return new OnlineOrdersOutboxState();
				default:
					throw new ArgumentOutOfRangeException(nameof(busName), busName, "Для шины не настроены таблицы outbox");
			}
		}

		public static OutboxMessageRecord CreateMessage(string busName)
		{
			switch(busName)
			{
				case OutboxBusNames.Pacs:
					return new PacsOutboxMessage();
				case OutboxBusNames.Notifications:
					return new NotificationsOutboxMessage();
				case OutboxBusNames.Email:
					return new EmailOutboxMessage();
				case OutboxBusNames.Taxcom:
					return new TaxcomOutboxMessage();
				case OutboxBusNames.OnlineOrders:
					return new OnlineOrdersOutboxMessage();
				default:
					throw new ArgumentOutOfRangeException(nameof(busName), busName, "Для шины не настроены таблицы outbox");
			}
		}
	}
}
