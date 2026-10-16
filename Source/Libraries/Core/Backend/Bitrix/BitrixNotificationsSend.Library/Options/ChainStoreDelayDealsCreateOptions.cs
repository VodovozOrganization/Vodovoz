using System;

namespace BitrixNotificationsSend.Library.Options
{
	/// <summary>
	/// Настройки создания сделок по отсрочке сети
	/// </summary>
	public class ChainStoreDelayDealsCreateOptions
	{
		/// <summary>
		/// Интервал проверки необходимости создания сделок по отсрочке сети
		/// </summary>
		public TimeSpan Interval { get; set; }

		/// <summary>
		/// Время начала интервала создания сделок (по Москве)
		/// </summary>
		public TimeSpan SendTimeFrom { get; set; }

		/// <summary>
		/// Время окончания интервала создания сделок (по Москве)
		/// </summary>
		public TimeSpan SendTimeTo { get; set; }
	}
}
