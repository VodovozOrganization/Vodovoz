using FuelControl.Contracts.Responses;
using FuelControl.Library.Converters;
using FuelControl.Library.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;
using Vodovoz.Settings.Fuel;

namespace FuelControl.Library.Services
{
	public class GazpromTransactionsDataService : IFuelControlTransactionsDataService
	{
		private const string _requestDateTimeFormatString = "yyyy-MM-dd";
		private const string _transactionsEndpointAddress = "vip/v2/transactions";

		private readonly ILogger<GazpromTransactionsDataService> _logger;
		private readonly ITransactionConverter _transactionConverter;
		private readonly IFuelControlSettings _fuelControlSettings;

		public GazpromTransactionsDataService(
			ILogger<GazpromTransactionsDataService> logger,
			ITransactionConverter transactionConverter,
			IFuelControlSettings fuelControlSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_transactionConverter = transactionConverter ?? throw new System.ArgumentNullException(nameof(transactionConverter));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
		}

		public async Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPeriod(
			string sessionId,
			string apiKey,
			DateTime startDate,
			DateTime endDate,
			CancellationToken cancellationToken,
			int pageLimit = 500,
			int pageOffset = 0)
		{
			if(string.IsNullOrWhiteSpace(sessionId))
			{
				throw new ArgumentException($"'{nameof(sessionId)}' cannot be null or whitespace.", nameof(sessionId));
			}

			if(apiKey is null)
			{
				throw new ArgumentNullException(nameof(apiKey));
			}

			_logger.LogDebug(
				"Запрос на получение данных по тразакциям за период от {StartDate} по {EndDate}. 'PageLimit'={PageLimit}, 'PageOffset'={PageOffset}",
				startDate,
				endDate,
				pageLimit,
				pageOffset);

			var formatedStartDate = startDate.ToString(_requestDateTimeFormatString);
			var formatedEndDate = endDate.ToString(_requestDateTimeFormatString);

			if(endDate.Date < startDate.Date)
			{
				var message = $"Дата конца периода {formatedEndDate} не должна быть меньше даты начала периода {formatedStartDate}";

				_logger.LogError(message);

				throw new ArgumentException(message, nameof(endDate));
			}

			var baseAddress = new Uri(_fuelControlSettings.ApiBaseAddress);

			using(var httpClient = new HttpClient { BaseAddress = baseAddress })
			{
				httpClient.Timeout = TimeSpan.FromSeconds(_fuelControlSettings.ApiRequesTimeout.TotalSeconds);
				httpClient.DefaultRequestHeaders.Add("api_key", apiKey);
				httpClient.DefaultRequestHeaders.Add("session_id", sessionId);
				httpClient.DefaultRequestHeaders.Add("contract_id", _fuelControlSettings.OrganizationContractId);

				var response = await httpClient.GetAsync(
					  $"{_transactionsEndpointAddress}?date_from={formatedStartDate}&date_to={formatedEndDate}&page_limit={pageLimit}&page_offset={pageOffset}",
					  cancellationToken);

				var responseString = await response.Content.ReadAsStringAsync();

				var responseData = JsonSerializer.Deserialize<TransactionsResponse>(responseString);

				if(responseData.Status.Errors?.Count() > 0)
				{
					var errorMessages =
						$"На запрос получения транзакций сервер Газпром вернул ответ с ошибками: {string.Concat(responseData.Status.Errors.Select(e => $"Тип: {e.ErrorType}. Сообщение: {e.Message}"))}";

					_logger.LogError(errorMessages);

					throw new FuelControlException(errorMessages);
				}

				_logger.LogDebug("Количество полученных транзакций: {TransactionsCount}",
					responseData.TransactionsData.Transactions?.Count());

				var transactions = ConvertResponseDataToTransactions(responseData);

				return transactions;
			}
		}

		private IEnumerable<FuelTransaction> ConvertResponseDataToTransactions(TransactionsResponse responseData)
		{
			var transactionDtos = responseData?.TransactionsData?.Transactions;

			if(transactionDtos == null)
			{
				return Enumerable.Empty<FuelTransaction>();
			}

			var transactions = transactionDtos
					.Select(t => _transactionConverter.ConvertToDomainFuelTransaction(t));

			return transactions;
		}
	}
}
