using System;

namespace EmailDebtNotificationWorker.Options
{
	/// <summary>
	/// Настройки воркера по рассылке писем о претензиях
	/// </summary>
	public class EmailClaimLettersOptions
	{
		/// <summary>
		/// Количество дней сверх ПДЗ до отправки претензионного письма
		/// </summary>
		public int LettersOfClaimTimeoutDays { get; set; }

		/// <summary>
		/// Интервал работы воркера по рассылке претензионных писем
		/// </summary>
		public TimeSpan WorkerInterval { get; set; }

		/// <summary>
		/// Максимальное количество претензионных писем, которое может быть отправлено за один цикл воркера
		/// </summary>
		public int MaxCountPerInterval { get; set; }

		/// <summary>
		/// Максимальное количество претензионных писем, которое может быть отправлено за один день
		/// </summary>
		public int MaxCountPerDay { get; set; }

		/// <summary>
		/// Интервал повторной отправки письма о претензии, если долг не был погашен после предыдущей отправки, в днях
		/// </summary>
		public int ResendIntervalDays { get; set; }
	}
}
