using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Services
{
	public interface IFuelControlTransactionsDataService
	{
		Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPeriod(string sessionId, string apiKey, 
			DateTime startDate, DateTime endDate, CancellationToken cancellationToken, int pageLimit = 500, int pageOffset = 0);
	}
}
