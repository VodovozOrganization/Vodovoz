using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class ReceiptSendingFailedEdoNotificationSettingMap : SubclassMap<ReceiptSendingFailedEdoNotificationSetting>
	{
		public ReceiptSendingFailedEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(ReceiptSendingFailedEdoNotificationSetting));
		}
	}
}
