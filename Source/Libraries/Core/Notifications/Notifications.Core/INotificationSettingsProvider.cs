namespace Notifications.Core
{
	/// <summary>
	/// Настройки отправки уведомлений: можно ли отправлять уведомление и можно ли отправлять его повторно
	/// </summary>
	/// <typeparam name="TEvent">Доменное событие уведомления</typeparam>
	public interface INotificationSettingsProvider<in TEvent>
	{
		/// <summary>
		/// Настройки отправки уведомления, одним запросом
		/// </summary>
		/// <param name="notificationEvent">Уведомление</param>
		NotificationSendingSettings GetSendingSettings(TEvent notificationEvent);
	}
}
