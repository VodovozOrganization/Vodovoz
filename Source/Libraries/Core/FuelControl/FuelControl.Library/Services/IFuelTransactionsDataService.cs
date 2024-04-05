using System;
using System.Collections.Generic;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Services
{
	public interface IFuelTransactionsDataService
	{
		IEnumerable<FuelTransaction> GetFuelTransactionsForPreviousDay();
		IEnumerable<FuelTransaction> GetFuelTransactionsForPreviousMonth();
		IEnumerable<FuelTransaction> GetFuelTransactionsForDate(DateTime date);
		IEnumerable<FuelTransaction> GetFuelTransactionsForPeriod(DateTime startDate, DateTime endDate);
	}
}
