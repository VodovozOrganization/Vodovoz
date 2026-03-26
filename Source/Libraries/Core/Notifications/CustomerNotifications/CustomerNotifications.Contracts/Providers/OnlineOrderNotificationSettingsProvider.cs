using System.Collections.Generic;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Contracts.Providers
{
	public class OnlineOrderNotificationSettingsProvider : IOnlineOrderNotificationSettingsProvider
	{
		private readonly IReadOnlyDictionary<CustomerNotificationEventType, OnlineOrderNotificationSetting> _settings;

		public OnlineOrderNotificationSettingsProvider(
			IReadOnlyDictionary<CustomerNotificationEventType, OnlineOrderNotificationSetting> settings)
		{
			_settings = settings;
		}

		public bool IsDuplicateAllowed(CustomerNotificationEventType eventType) =>
			_settings.TryGetValue(eventType, out var s) && s.AllowDuplicateNotifications;

		public bool IsDisabled(CustomerNotificationEventType eventType) =>
			_settings.TryGetValue(eventType, out var s) && s.NotificationDisabled;

		public string GetNotificationText(CustomerNotificationEventType eventType) =>
			_settings.TryGetValue(eventType, out var s) ? s.NotificationText : null;
	}
}
