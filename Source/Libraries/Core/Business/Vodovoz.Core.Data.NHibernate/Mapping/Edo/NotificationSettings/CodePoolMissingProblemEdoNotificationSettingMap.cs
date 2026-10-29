using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class CodePoolMissingProblemEdoNotificationSettingMap : SubclassMap<CodePoolMissingProblemEdoNotificationSetting>
	{
		public CodePoolMissingProblemEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(CodePoolMissingProblemEdoNotificationSetting));
		}
	}
}
