using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.NotificationSettings;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo.NotificationSettings
{
	public class OrderSelfDeliveryPaymentProblemEdoNotificationSettingMap : SubclassMap<OrderSelfDeliveryPaymentProblemEdoNotificationSetting>
	{
		public OrderSelfDeliveryPaymentProblemEdoNotificationSettingMap()
		{
			DiscriminatorValue(nameof(OrderSelfDeliveryPaymentProblemEdoNotificationSetting));
		}
	}
}
