using CustomerPushNotifications.Contracts;
using TransactionalOutbox.Abstractions;

namespace CustomerPushNotifications.Application.Providers
{
	public interface ICustomerPushNotificationsSettingsProvider : IOutBoxSettingsProvider<CustomerNotificationDomainEvent>
	{
		CustomerNotificationPushType GetCustomerPushType(CustomerNotificationDomainEvent notificationDomainEvent);
		CustomerNotificationTargetType GetCustomerPushTarget(CustomerNotificationDomainEvent notificationDomainEvent);
		/// <summary>
		/// Получение текста уведомления для данного типа события
		/// </summary>
		/// <param name="notificationEvent">Тип события</param>
		/// <returns></returns>
		string GetNotificationText(CustomerNotificationDomainEvent notificationEvent);
	}
}
