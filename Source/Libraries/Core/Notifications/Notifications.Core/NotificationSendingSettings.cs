namespace Notifications.Core
{
	/// <summary>
	/// Настройки отправки уведомления, которые проверяет публикатор
	/// </summary>
	public sealed class NotificationSendingSettings
	{
		public NotificationSendingSettings(bool isDisabled, bool isDuplicateAllowed)
		{
			IsDisabled = isDisabled;
			IsDuplicateAllowed = isDuplicateAllowed;
		}

		/// <summary>
		/// Отправка уведомления отключена
		/// </summary>
		public bool IsDisabled { get; }

		/// <summary>
		/// Можно отправить повторно, если ключ дедупликации уже зарегистрирован
		/// </summary>
		public bool IsDuplicateAllowed { get; }
	}
}
