using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Orders
{
	public class OnlineOrderNotificationSettingMap : ClassMap<OnlineOrderNotificationSetting>
	{
		public OnlineOrderNotificationSettingMap()
		{
			Table("online_order_notification_settings");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.NotificationText)
				.Column("notification_text");

			Map(x => x.CustomerNotificationEventType)
				.Column("notification_event_type");

			Map(x => x.NotificationClassification)
				.Column("notification_classification");
			
			Map(x => x.NotificationDisabled)
				.Column("notification_disabled");
			
			Map(x => x.AllowDuplicateNotifications)
				.Column("allow_duplicate_notifications");
		}
	}
}
