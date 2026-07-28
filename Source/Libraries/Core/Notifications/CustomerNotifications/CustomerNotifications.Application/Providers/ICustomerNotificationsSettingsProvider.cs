using CustomerNotifications.Contracts;
using TransactionalOutbox.Abstractions;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Application.Providers
{
	/// <summary>
	/// Провайдер настроек для уведомлений клиентов.
	/// </summary>
	public interface ICustomerNotificationsSettingsProvider : IOutboxSettingsProvider<CustomerNotificationDomainEvent>
	{
		/// <summary>
		/// Получение типа пуш-уведомления для данного типа события
		/// </summary>
		/// <param name="notificationDomainEvent"></param>
		/// <returns></returns>
		CustomerNotificationPushType GetCustomerPushType(CustomerNotificationDomainEvent notificationDomainEvent);

		/// <summary>
		/// Получение типа цели пуш-уведомления для данного типа события
		/// </summary>
		/// <param name="notificationDomainEvent"></param>
		/// <returns></returns>
		CustomerNotificationTargetType GetCustomerPushTarget(CustomerNotificationDomainEvent notificationDomainEvent);

		/// <summary>
		/// Получение текста уведомления для данного типа события
		/// </summary>
		/// <param name="notificationEvent">Тип события</param>
		/// <returns></returns>
		string GetNotificationText(CustomerNotificationDomainEvent notificationEvent);
	}
}
