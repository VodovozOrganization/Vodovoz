namespace CustomerOnlineOrdersStatusUpdateNotifier.Configs
{
	/// <summary>
	/// Настройки для отправки уведомлений на сайт ВВ
	/// </summary>
	public class VodovozWebSiteUriOptions
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
