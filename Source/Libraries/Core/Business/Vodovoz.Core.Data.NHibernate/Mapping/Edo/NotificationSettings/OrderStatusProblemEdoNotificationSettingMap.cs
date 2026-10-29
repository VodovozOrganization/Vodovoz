using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class OrderStatusProblemEdoNotificationSettingMap : SubclassMap<OrderStatusProblemEdoNotificationSetting>
	{
		public OrderStatusProblemEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(OrderStatusProblemEdoNotificationSetting));
		}
	}
}
