using System;

namespace BitrixNotificationsSend.Library.Options
{
	/// <summary>
	/// Настройки создания сделок по плановым заказам клиентов
	/// </summary>
	public class PlannedOrdersDealsCreateOptions
	{
		/// <summary>
		/// Интервал проверки необходимости создания сделок по плановым заказам
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
