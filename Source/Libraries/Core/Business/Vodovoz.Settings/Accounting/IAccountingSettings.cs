using System;
using System.Collections.Generic;

namespace Vodovoz.Settings.Accounting
{
	public interface IAccountingSettings
	{
		/// <summary>
		/// Получение дат закрытия бухгалтерского периода
		/// </summary>
		/// <returns></returns>
		IEnumerable<DateTime> GetAccountingPeriodClosingDates();

		/// <summary>
		/// Сохранение четырёх дат закрытия бухгалтерского периода в параметр accounting_period_closing_dates
		/// </summary>
		void UpdateAccountingPeriodClosingDates(DateTime firstQuarter, DateTime secondQuarter, DateTime thirdQuarter, DateTime fourthQuarter);
	}
}
