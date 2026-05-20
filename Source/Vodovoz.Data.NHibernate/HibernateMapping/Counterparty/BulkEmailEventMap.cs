using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class BulkEmailEventMap : ClassMap<BulkEmailEvent>
	{
		public BulkEmailEventMap()
		{
			Table("bulk_email_events");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("event_type");

			Map(x => x.EventType).Column("event_type").ReadOnly();
			Map(x => x.CounterpartyEmailType).Column("counterparty_email_type");
			Map(x => x.ActionTime).Column("action_time").ReadOnly();
			Map(x => x.ReasonDetail).Column("reason_detail");

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.Reason).Column("bulk_email_event_reason_id");
		}

		public class SubscribingBulkEmailEventMap : SubclassMap<SubscribingBulkEmailEvent>
		{
			public SubscribingBulkEmailEventMap()
			{
				DiscriminatorValue(nameof(BulkEmailEventType.Subscribing));
			}
		}

		public class UnsubscribingBulkEmailEventMap : SubclassMap<UnsubscribingBulkEmailEvent>
		{
			public UnsubscribingBulkEmailEventMap()
			{
				DiscriminatorValue(nameof(BulkEmailEventType.Unsubscribing));
			}
		}
	}
}
