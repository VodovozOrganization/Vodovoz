using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class DocflowDiscrepancyEdoNotificationSettingMap : SubclassMap<DocflowDiscrepancyEdoNotificationSetting>
	{
		public DocflowDiscrepancyEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(DocflowDiscrepancyEdoNotificationSetting));
		}
	}
}
