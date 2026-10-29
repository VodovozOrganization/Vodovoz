using CustomerNotifications.Contracts;
using Notifications.Core;
using Vodovoz.Core.Domain.Orders;

namespace CustomerNotifications.Application.Providers
{
	/// <summary>
	/// Провайдер настроек для уведомлений клиентов.
	/// </summary>
	public interface ICustomerNotificationsSettingsProvider : INotificationSettingsProvider<CustomerNotificationDomainEvent>
	{
		/// <summary>
		/// Настройка уведомления для типа события: текст, тип и цель пуша, отключение и повторы.
		/// Одним запросом к БД; если настройки нет — исключение
		/// </summary>
		OnlineOrderNotificationSetting GetSetting(CustomerNotificationDomainEvent notificationEvent);
	}
}
