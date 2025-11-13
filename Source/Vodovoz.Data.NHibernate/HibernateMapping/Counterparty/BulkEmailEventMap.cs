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

			DiscriminateSubClassesOnColumn("type");

			Map(x => x.Type).Column("type").ReadOnly();
			Map(x => x.ActionTime).Column("action_time").ReadOnly();
			Map(x => x.ReasonDetail).Column("reason_detail");

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.Reason).Column("bulk_email_event_reason_id");
		}

		public class SubscribingBulkEmailEventMap : SubclassMap<SubscribingBulkEmailEvent>
		{
			public SubscribingBulkEmailEventMap()
			{
				DiscriminatorValue(nameof(BulkEmailEvent.BulkEmailEventType.Subscribing));
			}
		}

		public class UnsubscribingBulkEmailEventMap : SubclassMap<UnsubscribingBulkEmailEvent>
		{
			public UnsubscribingBulkEmailEventMap()
			{
				DiscriminatorValue(nameof(BulkEmailEvent.BulkEmailEventType.Unsubscribing));
			}
		}
	}
}
