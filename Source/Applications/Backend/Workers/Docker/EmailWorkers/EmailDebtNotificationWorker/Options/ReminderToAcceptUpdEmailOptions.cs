using System;

namespace EmailDebtNotificationWorker.Options
{
	/// <summary>
	/// Опции воркера отправки писем с напоминанием о необходимости принятия УПД
	/// </summary>
	public class ReminderToAcceptUpdEmailOptions
	{
		public const string SectionName = "ReminderToAcceptUpdEmailOptions";

		/// <summary>
		/// Время запуска воркера отправки писем с напоминанием о необходимости принятия УПД
		/// </summary>
		public TimeSpan StartTime { get; set; }

		/// <summary>
		/// Количество дней, по истечении которых будет отправлено письмо с напоминанием о необходимости принятия УПД
		/// </summary>
		public int TimeoutDays { get; set; }
	}
}
