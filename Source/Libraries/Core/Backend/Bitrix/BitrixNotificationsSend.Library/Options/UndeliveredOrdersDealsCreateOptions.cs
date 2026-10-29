using System;

namespace BitrixNotificationsSend.Library.Options
{
	/// <summary>
	/// Настройки создания сделок по недовозам.
	/// </summary>
	public class UndeliveredOrdersDealsCreateOptions
	{
		/// <summary>
		/// Интервал проверки необходимости создания сделок по недовозам.
		/// </summary>
		public TimeSpan Interval { get; set; }

		/// <summary>
		/// Минимальное время изменения недовоза для попадания в синхронизацию.
		/// </summary>
		public DateTime MinLastEditedTime { get; set; }
	}
}
