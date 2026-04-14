using CustomerNotifications.Contracts;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Application.Providers
{
	public class CustomerNotificationsSettingsProvider : ICustomerNotificationsSettingsProvider
	{
		private readonly IReadOnlyDictionary<CustomerNotificationEventType, OnlineOrderNotificationSetting> _settings;

		public CustomerNotificationsSettingsProvider(
			IReadOnlyDictionary<CustomerNotificationEventType, OnlineOrderNotificationSetting> settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		public bool IsDuplicateAllowed(CustomerNotificationDomainEvent notificationDomainEvent)
		{
			return _settings.TryGetValue(notificationDomainEvent.CustomerNotificationEventType, out var setting)
			       && setting.AllowDuplicateNotifications;
		}

		public bool IsDisabled(CustomerNotificationDomainEvent notificationDomainEvent)
		{
			return _settings.TryGetValue(notificationDomainEvent.CustomerNotificationEventType, out var setting)
			       && setting.NotificationDisabled;
		}

		public string GetNotificationText(CustomerNotificationDomainEvent notificationDomainEvent)
		{
			return _settings.TryGetValue(notificationDomainEvent.CustomerNotificationEventType, out var setting)
				? setting.NotificationText
				: null;
		}

		public CustomerNotificationPushType GetCustomerPushType(CustomerNotificationDomainEvent notificationDomainEvent)
		{
			_settings.TryGetValue(notificationDomainEvent.CustomerNotificationEventType, out var setting);
			return setting.PushType;
		}

		public CustomerNotificationTargetType GetCustomerPushTarget(CustomerNotificationDomainEvent notificationDomainEvent)
		{
			_settings.TryGetValue(notificationDomainEvent.CustomerNotificationEventType, out var setting);
			return setting.PushTarget;
		}
	}
}
