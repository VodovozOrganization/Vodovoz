using CustomerNotifications.Contracts;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Application.Providers
{
	public class CustomerNotificationsSettingsProvider : ICustomerNotificationsSettingsProvider
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public CustomerNotificationsSettingsProvider(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		private OnlineOrderNotificationSetting GetSetting(CustomerNotificationDomainEvent notificationDomainEvent)
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

		public bool IsDuplicateAllowed(CustomerNotificationDomainEvent notificationDomainEvent)
			=> GetSetting(notificationDomainEvent)?.AllowDuplicateNotifications ?? false;

		public bool IsDisabled(CustomerNotificationDomainEvent notificationDomainEvent)
			=> GetSetting(notificationDomainEvent)?.NotificationDisabled ?? true;

		public string GetNotificationText(CustomerNotificationDomainEvent notificationDomainEvent)
			=> GetSetting(notificationDomainEvent).NotificationText;

		public CustomerNotificationPushType GetCustomerPushType(CustomerNotificationDomainEvent notificationDomainEvent)
			=> GetSetting(notificationDomainEvent).PushType;

		public CustomerNotificationTargetType GetCustomerPushTarget(CustomerNotificationDomainEvent notificationDomainEvent)
			=> GetSetting(notificationDomainEvent).PushTarget;
	}
}
