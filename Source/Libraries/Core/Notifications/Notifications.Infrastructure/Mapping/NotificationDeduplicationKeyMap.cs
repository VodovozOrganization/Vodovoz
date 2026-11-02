using FluentNHibernate.Mapping;
using Notifications.Core;

namespace Notifications.Infrastructure.Mapping
{
	public class NotificationDeduplicationKeyMap : ClassMap<NotificationDeduplicationKey>
	{
		public NotificationDeduplicationKeyMap()
		{
			Table("notification_deduplication_keys");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Key).Column("deduplication_key");
			Map(x => x.CreatedAt).Column("created_at");
		}
	}
}
