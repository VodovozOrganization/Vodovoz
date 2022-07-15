using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class BulkEmailEventMap : ClassMap<BulkEmailEvent>
	{
		public BulkEmailEventMap()
		{
			Table("bulk_email_events");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("type");

			Map(x => x.Type).Column("type").CustomType<BulkEmailEvent.BulkEmailEventTypeString>().ReadOnly();
			Map(x => x.ActionTime).Column("action_time").ReadOnly();

			References(x => x.Counterparty).Column("counterparty_id");
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
				Map(x => x.OtherReason).Column("other_reason");
				References(x => x.UnsubscribingReason).Column("unsubscribing_reason_id");
			}
		}
	}
}
