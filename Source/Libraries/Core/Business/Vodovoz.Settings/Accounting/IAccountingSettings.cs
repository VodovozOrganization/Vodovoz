using System;
using System.Collections.Generic;

namespace Vodovoz.Settings.Accounting
{
	public interface IAccountingSettings
	{
		IEnumerable<DateTime> GetAccountingPeriodClosingDates();
	}
}
