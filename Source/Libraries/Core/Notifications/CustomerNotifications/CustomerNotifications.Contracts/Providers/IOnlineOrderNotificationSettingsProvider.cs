using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Contracts.Providers
{
    /// <summary>
    /// Предоставляет настройки уведомлений для онлайн-заказов
    /// </summary>
    public interface IOnlineOrderNotificationSettingsProvider
	{
		/// <summary>
		/// Разрешены ли дубликаты сообщений для типа события
		/// </summary>
		/// <param name="eventType">Тип события</param>
		/// <returns></returns>
		bool IsDuplicateAllowed(CustomerNotificationEventType eventType);

		/// <summary>
		/// Отключена ли отправка для данного типа события
		/// </summary>
		/// <param name="eventType">Тип события</param>
		/// <returns></returns>
		bool IsDisabled(CustomerNotificationEventType eventType);

		/// <summary>
		/// Получение тексат уведомления для данного типа события
		/// </summary>
		/// <param name="eventType">Тип события</param>
		/// <returns></returns>
		string GetNotificationText(CustomerNotificationEventType eventType);
	}
}
