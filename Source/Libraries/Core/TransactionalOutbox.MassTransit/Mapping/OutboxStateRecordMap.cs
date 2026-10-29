using FluentNHibernate.Mapping;
using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Маппинг на таблицу OutboxState MassTransit (колонки как в EF Core-модели MassTransit).
	/// NHibernate только добавляет строки (UnitOfWorkOutboxContext), читает и удаляет их служба отправки MassTransit.
	/// RowVersion не маппится: его заполняет БД. После перехода на один vhost RabbitMQ маппинги по шинам (папка Buses) сведутся к одному
	/// </summary>
	public abstract class OutboxStateRecordMap<TRecord> : ClassMap<TRecord>
		where TRecord : OutboxStateRecord
	{
		protected OutboxStateRecordMap(string busName)
		{
			Table($"{busName}_outbox_state");

			Id(x => x.OutboxId).Column("OutboxId").GeneratedBy.Assigned();
			Map(x => x.LockId).Column("LockId");
			Map(x => x.Created).Column("Created");
		}
	}
}
