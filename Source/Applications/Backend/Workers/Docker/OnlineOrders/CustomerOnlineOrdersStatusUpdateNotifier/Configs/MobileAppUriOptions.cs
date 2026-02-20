namespace CustomerOnlineOrdersStatusUpdateNotifier.Configs
{
	/// <summary>
	/// Настройки для отправки уведомлений в МП
	/// </summary>
	public class MobileAppUriOptions
	{
		/// <summary>
		/// Базовый адрес
		/// </summary>
		public string BaseUrl { get; set; }
		/// <summary>
		/// Эндпойнт
		/// </summary>
		public string NotificationAddress { get; set; }
	}
}
