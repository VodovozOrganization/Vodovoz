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
	public class GazpromFuelCardsDataService : IFuelControlFuelCardsDataService
	{
		private const string _requestDateTimeFormatString = "yyyy-MM-dd HH:mm:ss";
		private const string _cardsEndpointAddress = "vip/v2/cards";

		private readonly ILogger<GazpromFuelCardsDataService> _logger;
		private readonly IFuelCardConverter _fuelCardConverter;
		private readonly IFuelControlSettings _fuelControlSettings;

		public GazpromFuelCardsDataService(
			ILogger<GazpromFuelCardsDataService> logger,
			IFuelCardConverter fuelCardConverter,
			IFuelControlSettings fuelControlSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fuelCardConverter = fuelCardConverter ?? throw new ArgumentNullException(nameof(fuelCardConverter));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
		}

		public async Task<IEnumerable<FuelCard>> GetFuelCards(
			string sessionId,
			string apiKey,
			CancellationToken cancellationToken,
			int pageLimit = 500,
			int pageOffset = 0)
		{
			if(string.IsNullOrWhiteSpace(sessionId))
			{
				throw new ArgumentException($"'{nameof(sessionId)}' cannot be null or whitespace.", nameof(sessionId));
			}

			if(string.IsNullOrWhiteSpace(apiKey))
			{
				throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or whitespace.", nameof(apiKey));
			}

			_logger.LogDebug(
				"Запрос на получение списка карт договора. 'PageLimit'={PageLimit}, 'PageOffset'={PageOffset}",
				pageLimit,
				pageOffset);

			var baseAddress = new Uri(_fuelControlSettings.ApiBaseAddress);

			using(var httpClient = new HttpClient { BaseAddress = baseAddress })
			{
				httpClient.Timeout = TimeSpan.FromSeconds(_fuelControlSettings.ApiRequesTimeout.TotalSeconds);
				httpClient.DefaultRequestHeaders.Add("api_key", apiKey);
				httpClient.DefaultRequestHeaders.Add("session_id", sessionId);
				httpClient.DefaultRequestHeaders.Add("date_time", DateTime.Now.ToString(_requestDateTimeFormatString));
				httpClient.DefaultRequestHeaders.Add("contract_id", _fuelControlSettings.OrganizationContractId);

				var response = await httpClient.GetAsync(
					  $"{_cardsEndpointAddress}",
					  cancellationToken);

				var responseString = await response.Content.ReadAsStringAsync();

				var responseData = JsonSerializer.Deserialize<FuelCardResponse>(responseString);

				if(responseData.Status.Errors?.Count() > 0)
				{
					var errorMessages =
						$"На запрос получения списка карт сервер Газпром вернул ответ с ошибками: {string.Concat(responseData.Status.Errors.Select(e => $"Тип: {e.ErrorType}. Сообщение: {e.Message}"))}";

					_logger.LogError(errorMessages);

					throw new FuelControlException(errorMessages);
				}

				_logger.LogDebug("Количество полученных карт: {TransactionsCount}",
					responseData.FuelCardsData.FuelCards?.Count());

				var transactions = ConvertResponseDataToTransactions(responseData);

				return transactions;
			}
		}

		private IEnumerable<FuelCard> ConvertResponseDataToTransactions(FuelCardResponse responseData)
		{
			var fuelCardDtos = responseData?.FuelCardsData?.FuelCards;

			if(fuelCardDtos == null)
			{
				return Enumerable.Empty<FuelCard>();
			}

			var fuelCards = fuelCardDtos
					.Select(t => _fuelCardConverter.ConvertToDomainFuelCard(t));

			return fuelCards;
		}
	}
}
