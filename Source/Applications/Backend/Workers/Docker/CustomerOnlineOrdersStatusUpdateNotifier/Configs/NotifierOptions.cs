namespace CustomerOnlineOrdersStatusUpdateNotifier.Configs
{
	/// <summary>
	/// Настройки сервиса уведомлений
	/// </summary>
	public class NotifierOptions
	{
		public const string Path = "NotifierOptions";
		/// <summary>
		/// Задержка между запусками в секундах
		/// </summary>
		public int DelayInSeconds { get; set; }
		/// <summary>
		/// Таймаут отправки
		/// </summary>
		public int SendingTimeoutInSeconds { get; set; }
		/// <summary>
		/// Количество предыдущих дней для выборки
		/// </summary>
		public int PastDaysForSend { get; set; }
		/// <summary>
		/// Количество отсылаемых сообщений за раз
		/// </summary>
		public int NotificationCountInSession { get; set; }
		/// <summary>
		/// Настройки для отправки в МП
		/// </summary>
		public MobileAppUriOptions MobileAppUriOptions { get; set; }
		/// <summary>
		/// Настройки для отправки на сайт ВВ
		/// </summary>
		public VodovozWebSiteUriOptions VodovozWebSiteUriOptions { get; set; }
	}
}
