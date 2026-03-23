using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Contracts.Providers
{
	public interface IOnlineOrderNotificationSettingsProvider
	{
		bool IsDuplicateAllowed(CustomerNotificationEventType eventType);
		bool IsDisabled(CustomerNotificationEventType eventType);
		string GetNotificationText(CustomerNotificationEventType eventType);
	}
}
