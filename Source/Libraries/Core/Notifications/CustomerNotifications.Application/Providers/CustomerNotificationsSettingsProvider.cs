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

		private OnlineOrderNotificationSetting GetSetting(CustomerNotificationDomainEvent notificationDomainEvent)
		{
			if(notificationDomainEvent == null)
			{
				throw new ArgumentNullException(nameof(notificationDomainEvent));
			}

			if(!_settings.TryGetValue(notificationDomainEvent.CustomerNotificationEventType, out var setting))
			{
				throw new InvalidOperationException(
					$"Не найдена настройка для типа события '{notificationDomainEvent.CustomerNotificationEventType}'.");
			}

			return setting;
		}

		public bool IsDuplicateAllowed(CustomerNotificationDomainEvent notificationDomainEvent) => GetSetting(notificationDomainEvent)?.AllowDuplicateNotifications ?? false;

		public bool IsDisabled(CustomerNotificationDomainEvent notificationDomainEvent) => GetSetting(notificationDomainEvent)?.NotificationDisabled ?? true;

		public string GetNotificationText(CustomerNotificationDomainEvent notificationDomainEvent) => GetSetting(notificationDomainEvent).NotificationText;

		public CustomerNotificationPushType GetCustomerPushType(CustomerNotificationDomainEvent notificationDomainEvent) => GetSetting(notificationDomainEvent).PushType;
		
		public CustomerNotificationTargetType GetCustomerPushTarget(CustomerNotificationDomainEvent notificationDomainEvent) => GetSetting(notificationDomainEvent).PushTarget;		
	}
}
