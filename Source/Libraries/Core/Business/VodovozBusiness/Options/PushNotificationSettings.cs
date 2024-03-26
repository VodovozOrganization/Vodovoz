namespace Vodovoz.Options
{
	/// <summary>
	/// Настройки PUSH-сообщений
	/// </summary>
	public class PushNotificationSettings
	{
		/// <summary>
		/// Включены ли PUSH-сообщения
		/// </summary>
		public bool PushNotificationsEnabled { get; set; } = true;

		/// <summary>
		/// Включены ли WakeUp-сообщения
		/// </summary>
		public bool WakeUpNotificationsEnabled { get; set; } = true;
	}
}
