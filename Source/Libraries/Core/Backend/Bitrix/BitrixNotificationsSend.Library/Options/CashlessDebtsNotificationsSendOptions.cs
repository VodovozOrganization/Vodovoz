using System;

namespace BitrixNotificationsSend.Library.Options
{
	/// <summary>
	/// Настройки отправки уведомлений по компаниям с долгом по безналу
	/// </summary>
	public class CashlessDebtsNotificationsSendOptions
	{
		/// <summary>
		/// Интервал работы воркера отправки уведомлений по компаниям с долгом по безналу
		/// </summary>
		public TimeSpan Interval { get; set; }
	}
}
