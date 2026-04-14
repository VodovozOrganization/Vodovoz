using FluentNHibernate.Mapping;
using TransactionalOutbox.Domain;

namespace Vodovoz.Core.Data.NHibernate.Outbox
{
	public class OutboxMessageMap : ClassMap<OutboxMessage>
	{
		public OutboxMessageMap()
		{
			Table("outbox_messages");
			
			Id(x => x.Guid).Column("guid").GeneratedBy.Guid();
			Map(x => x.CreatedAt).Column("created_at").ReadOnly();
			Map(x => x.PayloadJson).Column("payload");
			Map(x => x.Type).Column("type");
			Map(x => x.SentAt).Column("sent_at");
			Map(x => x.Attempts).Column("attempts");
			Map(x => x.Error).Column("error");
			Map(x => x.DeduplicationKey).Column("deduplication_key");			
		}
	}
}
