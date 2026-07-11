using System;

namespace BitrixNotificationsSend.Library.Options
{
	/// <summary>
	/// Настройки отправки уведомлений по плановым заказам клиентов
	/// </summary>
	public class PlannedOrdersNotificationsSendOptions
	{
		/// <summary>
		/// Интервал проверки необходимости отправки уведомлений по плановым заказам
		/// </summary>
		public TimeSpan Interval { get; set; }

		/// <summary>
		/// Время начала интервала отправки уведомлений (по Москве)
		/// </summary>
		public TimeSpan SendTimeFrom { get; set; }

		/// <summary>
		/// Время окончания интервала отправки уведомлений (по Москве)
		/// </summary>
		public TimeSpan SendTimeTo { get; set; }
	}
}
