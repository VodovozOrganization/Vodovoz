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
	}
}
