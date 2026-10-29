using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class ReceiptQueueStalledEdoNotificationSettingMap : SubclassMap<ReceiptQueueStalledEdoNotificationSetting>
	{
		public ReceiptQueueStalledEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(ReceiptQueueStalledEdoNotificationSetting));
		}
	}
}
