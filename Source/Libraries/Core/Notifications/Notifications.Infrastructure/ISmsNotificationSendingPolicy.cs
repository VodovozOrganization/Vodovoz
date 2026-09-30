namespace Notifications.Infrastructure
{
	/// <summary>
	/// Политика отправки смс уведомлений
	/// Позволяет публикатору проверять глобальное разрешение на отправку смс
	/// </summary>
	public interface ISmsNotificationSendingPolicy
	{
		/// <summary>
		/// Разрешена ли отправка смс уведомлений
		/// </summary>
		bool IsSmsSendingEnabled { get; }
	}
}
