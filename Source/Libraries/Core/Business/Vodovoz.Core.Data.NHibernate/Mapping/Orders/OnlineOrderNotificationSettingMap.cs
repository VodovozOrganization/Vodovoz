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

			Map(x => x.ExternalOrderStatus)
				.Column("external_order_status");

			Map(x => x.NotificationText)
				.Column("notification_text");
		}
	}
}
