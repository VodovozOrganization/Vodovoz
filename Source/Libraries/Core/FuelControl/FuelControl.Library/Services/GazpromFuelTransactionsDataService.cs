using FuelControl.Contracts.Responses;
using FuelControl.Library.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;
using Vodovoz.Settings.Fuel;

namespace FuelControl.Library.Services
{
	public class GazpromFuelTransactionsDataService : IFuelTransactionsDataService
	{
		private readonly TransactionConverter _transactionConverter;
		private readonly IFuelControlSettings _fuelControlSettings;

		public GazpromFuelTransactionsDataService(TransactionConverter transactionConverter, IFuelControlSettings fuelControlSettings)
		{
			_transactionConverter = transactionConverter ?? throw new System.ArgumentNullException(nameof(transactionConverter));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
		}

		public async Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPeriod(
			string sessionId,
			string apiKey,
			DateTime startDate,
			DateTime endDate,
			int pageLimit = 500,
			int pageOffset = 0)
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
				var baseAddress = new Uri(_fuelControlSettings.ApiBaseAddress);
				var transactionList = new List<FuelTransaction>();

				using(var httpClient = new HttpClient { BaseAddress = baseAddress })
				{
					httpClient.DefaultRequestHeaders.Add("api_key", apiKey);
					httpClient.DefaultRequestHeaders.Add("session_id", sessionId);
					httpClient.DefaultRequestHeaders.Add("contract_id", _fuelControlSettings.OrganizationContractId);

					var response = await httpClient.GetAsync(
						  $"vip/v2/transactions?date_from={formatedStartDate}&date_to={formatedEndDate}&page_limit={pageLimit}&page_offset={pageOffset}");

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
