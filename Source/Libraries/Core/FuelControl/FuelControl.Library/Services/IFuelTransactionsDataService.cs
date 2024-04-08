using FuelControl.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;

namespace FuelControl.Library.Services
{
	public interface IFuelTransactionsDataService
	{
		Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPreviousDay(string sessionId, string baseAddressString, string apiKey, string contractId);
		Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPreviousMonth(string sessionId, string baseAddressString, string apiKey, string contractId);
		Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForDate(string sessionId, string baseAddressString, string apiKey, string contractId, DateTime date);
		Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPeriod(string sessionId, string baseAddressString, string apiKey, string contractId, DateTime startDate, DateTime endDate);
	}
}
