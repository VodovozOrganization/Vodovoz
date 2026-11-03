using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class CodeDuplicatedEdoNotificationSettingMap : SubclassMap<CodeDuplicatedEdoNotificationSetting>
	{
		public CodeDuplicatedEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(CodeDuplicatedEdoNotificationSetting));
		}
	}
}
