using FluentNHibernate.Mapping;
using NHibernate.Type;
using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit.Mapping
{
	/// <summary>
	/// Маппинг NHibernate на штатную таблицу OutboxMessage MassTransit (колонки как в EF Core-модели MassTransit).
	/// После перехода на один vhost RabbitMQ маппинги по шинам (папка Buses) сведутся к одному
	/// </summary>
	public abstract class OutboxMessageRecordMap<TRecord> : ClassMap<TRecord>
		where TRecord : OutboxMessageRecord
	{
		protected OutboxMessageRecordMap(string busName)
		{
			Table($"{busName}_outbox_message");

			Id(x => x.SequenceNumber).Column("SequenceNumber").GeneratedBy.Identity();
			Map(x => x.OutboxId).Column("OutboxId");
			Map(x => x.MessageId).Column("MessageId");
			Map(x => x.EnqueueTime).Column("EnqueueTime");
			Map(x => x.SentTime).Column("SentTime");
			Map(x => x.ExpirationTime).Column("ExpirationTime");
			Map(x => x.Headers).Column("Headers");
			Map(x => x.Properties).Column("Properties");
			Map(x => x.ContentType).Column("ContentType");
			Map(x => x.Body).Column("Body");
			Map(x => x.ConversationId).Column("ConversationId");
			Map(x => x.CorrelationId).Column("CorrelationId");
			Map(x => x.InitiatorId).Column("InitiatorId");
			Map(x => x.RequestId).Column("RequestId");
			Map(x => x.SourceAddress).Column("SourceAddress").CustomType<UriType>();
			Map(x => x.DestinationAddress).Column("DestinationAddress").CustomType<UriType>();
			Map(x => x.ResponseAddress).Column("ResponseAddress").CustomType<UriType>();
			Map(x => x.FaultAddress).Column("FaultAddress").CustomType<UriType>();
		}
	}
}
