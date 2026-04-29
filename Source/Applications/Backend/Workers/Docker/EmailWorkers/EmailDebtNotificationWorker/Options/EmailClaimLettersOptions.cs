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
		public int OverdueDebtorDebtExpiredDaysAgo { get; set; }

		/// <summary>
		/// Интервал работы воркера по рассылке претензионных писем
		/// </summary>
		public TimeSpan OverdueDebtorDebtInterval { get; set; }

		/// <summary>
		/// Максимальное количество претензионных писем, которое может быть отправлено за один цикл воркера
		/// </summary>
		public int OverdueDebtorDebtLettersCountPerInterval { get; set; }
	}
}
