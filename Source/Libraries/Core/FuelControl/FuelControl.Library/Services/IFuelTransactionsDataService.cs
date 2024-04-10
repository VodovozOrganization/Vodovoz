using FuelControl.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Services
{
	public interface IFuelTransactionsDataService
	{
		Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPeriod(string sessionId, string apiKey, DateTime startDate, DateTime endDate, int pageLimit, int pageOffset);
	}
}
