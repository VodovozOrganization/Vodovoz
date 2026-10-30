using System;

namespace EmailDebtNotificationWorker.Options
{
	/// <summary>
	/// Настройки воркера писем по заказам сетей и тендерных клиентов
	/// </summary>
	public class ChainStoreDelayLettersOptions
	{
		/// <summary>
		/// Интервал работы воркера
		/// </summary>
		public TimeSpan Interval { get; set; }

		/// <summary>
		/// Время начала окна отправки
		/// </summary>
		public TimeSpan SendTimeFrom { get; set; }

		/// <summary>
		/// Время конца окна отправки
		/// </summary>
		public TimeSpan SendTimeTo { get; set; }
	}
}
