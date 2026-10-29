using CustomerNotifications.Contracts;
using Notifications.Core;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Orders;

namespace CustomerNotifications.Application.Providers
{
	public class CustomerNotificationsSettingsProvider : ICustomerNotificationsSettingsProvider
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public CustomerNotificationsSettingsProvider(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public OnlineOrderNotificationSetting GetSetting(CustomerNotificationDomainEvent notificationDomainEvent)
		{
			if(notificationDomainEvent == null)
			{
				throw new ArgumentNullException(nameof(notificationDomainEvent));
			}

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var setting = uow.GetAll<OnlineOrderNotificationSetting>()
					.FirstOrDefault(s => s.CustomerNotificationEventType == notificationDomainEvent.CustomerNotificationEventType);

				if(setting == null)
				{
					throw new InvalidOperationException(
						$"Не найдена настройка для типа события '{notificationDomainEvent.CustomerNotificationEventType}'.");
				}

				return setting;
			}
		}

		public NotificationSendingSettings GetSendingSettings(CustomerNotificationDomainEvent notificationDomainEvent)
		{
			var setting = GetSetting(notificationDomainEvent);
			return new NotificationSendingSettings(setting.NotificationDisabled, setting.AllowDuplicateNotifications);
		}
	}
}
