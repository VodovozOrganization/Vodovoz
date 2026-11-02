using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class ReceiptContactInvalidEdoNotificationSettingMap : SubclassMap<ReceiptContactInvalidEdoNotificationSetting>
	{
		public ReceiptContactInvalidEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(ReceiptContactInvalidEdoNotificationSetting));
		}
	}
}
