using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class ExcessCodesEdoNotificationSettingMap : SubclassMap<ExcessCodesEdoNotificationSetting>
	{
		public ExcessCodesEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(ExcessCodesEdoNotificationSetting));
		}
	}
}
