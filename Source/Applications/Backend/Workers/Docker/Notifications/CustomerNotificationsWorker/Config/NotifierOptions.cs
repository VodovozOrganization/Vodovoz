namespace CustomerNotificationsWorker.Config
{
	/// <summary>
	/// Настройки сервиса уведомлений
	/// </summary>
	public class NotifierOptions
	{
		public const string Path = "NotifierOptions";
		/// <summary>
		/// Таймаут отправки
		/// </summary>
		public int SendingTimeoutInSeconds { get; set; }
		/// Настройки для отправки в МП
		/// </summary>
		public MobileAppUriOptions MobileAppUriOptions { get; set; }
		/// <summary>
		/// Настройки для отправки на сайт ВВ
		/// </summary>
		public VodovozWebSiteUriOptions VodovozWebSiteUriOptions { get; set; }
	}
}
