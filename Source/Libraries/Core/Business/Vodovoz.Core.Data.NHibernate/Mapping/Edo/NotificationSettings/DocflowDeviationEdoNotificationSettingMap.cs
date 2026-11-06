using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class DocflowDeviationEdoNotificationSettingMap : SubclassMap<DocflowDeviationEdoNotificationSetting>
	{
		public DocflowDeviationEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(DocflowDeviationEdoNotificationSetting));
		}
	}
}
