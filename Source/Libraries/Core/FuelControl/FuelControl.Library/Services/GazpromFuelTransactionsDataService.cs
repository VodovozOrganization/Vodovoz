using FuelControl.Contracts.Responses;
using FuelControl.Library.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
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

		public async Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPreviousDay(
			string sessionId,
			string baseAddressString,
			string apiKey,
			string contractId)
		{
			var date = DateTime.Today.AddDays(-1);

			return await GetFuelTransactionsForPeriod(sessionId, baseAddressString, apiKey, contractId, date, date);
		}

		public async Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPreviousMonth(
			string sessionId,
			string baseAddressString,
			string apiKey,
			string contractId)
		{
			var previousMonth = DateTime.Today.AddMonths(-1);

			var startDate = new DateTime(previousMonth.Year, previousMonth.Month, 1);
			var endDate = startDate.AddMonths(1).AddDays(-1);

			return await GetFuelTransactionsForPeriod(sessionId, baseAddressString, apiKey, contractId, startDate, endDate);
		}

		public async Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForDate(
			string sessionId,
			string baseAddressString,
			string apiKey,
			string contractId,
			DateTime date)
		{
			return new List<FuelTransaction>();
		}

		public async Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPeriod(
			string sessionId,
			string baseAddressString,
			string apiKey,
			string contractId,
			DateTime startDate,
			DateTime endDate)
		{
			var formatedStartDate = startDate.ToString("yyyy-MM-dd");
			var formatedEndDate = endDate.ToString("yyyy-MM-dd");

			if(endDate.Date < startDate.Date)
			{
				var message = $"Дата конца периода {formatedEndDate} не должна быть меньше даты начала периода {formatedStartDate}";

				throw new ArgumentException(message, nameof(endDate));
			}

			try
			{
				var baseAddress = new Uri(baseAddressString);
				var transactionList = new List<FuelTransaction>();

				using(var httpClient = new HttpClient { BaseAddress = baseAddress })
				{
					httpClient.DefaultRequestHeaders.Add("api_key", apiKey);
					httpClient.DefaultRequestHeaders.Add("session_id", sessionId);
					httpClient.DefaultRequestHeaders.Add("contract_id", contractId);

					var response = await httpClient.GetAsync($"vip/v2/transactions?date_from={formatedStartDate}&date_to={formatedEndDate}");
					var responseString = await response.Content.ReadAsStringAsync();

					var responseData = JsonSerializer.Deserialize<TransactionsResponse>(responseString);

					transactionList = responseData.TransactionsData.Transactions
						.Select(t => _transactionConverter.ConvertToDomainFuelTransaction(t))
						.ToList();
				}

				return transactionList;
			}
			catch(Exception ex)
			{
				throw ex;
			}
		}
	}
}
