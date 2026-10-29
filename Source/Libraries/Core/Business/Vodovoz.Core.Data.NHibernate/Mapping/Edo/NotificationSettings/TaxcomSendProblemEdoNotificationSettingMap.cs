using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class TaxcomSendProblemEdoNotificationSettingMap : SubclassMap<TaxcomSendProblemEdoNotificationSetting>
	{
		public TaxcomSendProblemEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(TaxcomSendProblemEdoNotificationSetting));
		}
	}
}
