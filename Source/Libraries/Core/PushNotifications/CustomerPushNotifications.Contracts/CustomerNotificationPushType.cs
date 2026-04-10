namespace CustomerPushNotifications.Contracts
{
	/// <summary>
	/// Тип пуш уведомления
	/// </summary>
	public enum CustomerNotificationPushType
	{
		/// Отправка без push уведомления
		Silent,
		
		/// Открыть раздел/страницу в МП
		Navigate,
		
		/// Открыть и обновить страницу
		NavigateRefresh
	}
}
