using FuelControl.Library.Converters;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Services
{
	public class GazpromFuelTransactionsDataService : IFuelTransactionsDataService
	{
		private readonly TransactionConverter _transactionConverter;

		public GazpromFuelTransactionsDataService(TransactionConverter transactionConverter)
		{
			_transactionConverter = transactionConverter ?? throw new System.ArgumentNullException(nameof(transactionConverter));
		}

		public IEnumerable<FuelTransaction> GetFuelTransactionsForPreviousDay()
		{
			return new List<FuelTransaction>();
		}

		public IEnumerable<FuelTransaction> GetFuelTransactionsForPreviousMonth()
		{
			return new List<FuelTransaction>();
		}

		public IEnumerable<FuelTransaction> GetFuelTransactionsForDate(DateTime date)
		{
			return new List<FuelTransaction>();
		}

		public IEnumerable<FuelTransaction> GetFuelTransactionsForPeriod(DateTime startDate, DateTime endDate)
		{
			return new List<FuelTransaction>();
		}
	}
}
