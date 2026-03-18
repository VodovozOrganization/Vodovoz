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
	public class GazpromFuelLimitsManagementService : IFuelLimitsManagementService
	{
		private const string _requestDateTimeFormatString = "yyyy-MM-dd HH:mm:ss";
		private const string _limitsListEndpointAddress = "vip/v1/limit";
		private const string _removeLimitEndpointAddress = "vip/v1/removeLimit";
		private const string _setLimitEndpointAddress = "vip/v1/setLimit";

		private readonly ILogger<GazpromFuelLimitsManagementService> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IFuelLimitConverter _fuelLimitConverter;
		private readonly IFuelControlSettings _fuelControlSettings;

		public GazpromFuelLimitsManagementService(
			ILogger<GazpromFuelLimitsManagementService> logger,
			IHttpClientFactory httpClientFactory,
			IFuelLimitConverter fuelLimitConverter,
			IFuelControlSettings fuelControlSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_fuelLimitConverter = fuelLimitConverter ?? throw new ArgumentNullException(nameof(fuelLimitConverter));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
		}

		public async Task<IEnumerable<FuelLimit>> GetFuelLimitsByCardId(
			string cardId,
			string sessionId,
			string apiKey,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(cardId))
			{
				throw new ArgumentException($"'{nameof(cardId)}' cannot be null or whitespace.", nameof(cardId));
			}

			if(string.IsNullOrWhiteSpace(sessionId))
			{
				throw new ArgumentException($"'{nameof(sessionId)}' cannot be null or whitespace.", nameof(sessionId));
			}

			if(string.IsNullOrWhiteSpace(apiKey))
			{
				throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or whitespace.", nameof(apiKey));
			}

			_logger.LogDebug(
				"Запрос на получение списка имеющихся лимитов по карте CardId={CardId}.",
				cardId);

			var httpClient = _httpClientFactory.CreateClient(GazpromHttpClientNames.WithTimeout);

			using (var request = new HttpRequestMessage(
				HttpMethod.Get,
				$"{_limitsListEndpointAddress}?contract_id={_fuelControlSettings.OrganizationContractId}&card_id={cardId}"))
			{
				request.Headers.Add("api_key", apiKey);
				request.Headers.Add("session_id", sessionId);
				request.Headers.Add("date_time", DateTime.Now.ToString(_requestDateTimeFormatString));

				var response = await httpClient.SendAsync(request, cancellationToken);

				var responseString = await response.Content.ReadAsStringAsync();

				var responseData = JsonSerializer.Deserialize<FuelLimitsResponse>(responseString);

				if(responseData.Status.Errors?.Count() > 0)
				{
					var errorMessages =
						$"На запрос получения списка лимитов сервер Газпром вернул ответ с ошибками: {string.Concat(responseData.Status.Errors.Select(e => $"Тип: {e.ErrorType}. Сообщение: {e.Message}"))}";

					_logger.LogError(errorMessages);

					throw new FuelControlException(errorMessages);
				}

				_logger.LogDebug("Количество полученных лимитов: {LimitsCount}",
					responseData.FuelLimitsData.FuelLimits?.Count());

				var fuelLimits = ConvertResponseDataToFuelLimits(responseData);

				return fuelLimits;
			}
		}

		private IEnumerable<FuelLimit> ConvertResponseDataToFuelLimits(FuelLimitsResponse responseData)
		{
			var fuelLimitDtos = responseData?.FuelLimitsData?.FuelLimits;

			if(fuelLimitDtos == null)
			{
				return Enumerable.Empty<FuelLimit>();
			}

			var limits = fuelLimitDtos
					.Select(t => _fuelLimitConverter.ConvertResponseDtoToFuelLimit(t));

			return limits;
		}

		public async Task<bool> RemoveFuelLimitById(
			string limitId,
			string sessionId,
			string apiKey,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(limitId))
			{
				throw new ArgumentException($"'{nameof(limitId)}' cannot be null or whitespace.", nameof(limitId));
			}

			if(string.IsNullOrWhiteSpace(sessionId))
			{
				throw new ArgumentException($"'{nameof(sessionId)}' cannot be null or whitespace.", nameof(sessionId));
			}

			if(string.IsNullOrWhiteSpace(apiKey))
			{
				throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or whitespace.", nameof(apiKey));
			}

			var httpContent = CreateRemoveLimitHttpContent(_fuelControlSettings.OrganizationContractId, limitId, apiKey, sessionId);

			_logger.LogDebug("Выполняется запрос удаления существующего лимита {LimitId}. Id сессии {SessionId}, ключ API {ApiKey}",
				limitId,
				sessionId,
				apiKey);

			var httpClient = _httpClientFactory.CreateClient(GazpromHttpClientNames.Default);
			var response = await httpClient.PostAsync(_removeLimitEndpointAddress, httpContent, cancellationToken);

			var responseString = await response.Content.ReadAsStringAsync();

			var responseData = JsonSerializer.Deserialize<RemoveFuelLimitResponse>(responseString);

			if(responseData.Status.Errors?.Count() > 0)
			{
				var errorMessage =
					$"На запрос удаления существующего лимита {limitId} сервер Газпром вернул ответ с ошибками: {string.Concat(responseData.Status.Errors.Select(e => $"\nТип: {e.ErrorType}. Сообщение: {e.Message}"))}";

				LogErrorMessageAndThrowException(errorMessage);
			}

			if(!responseData.IsRemovalSuccessful)
			{
				var errorMessage =
					$"На запрос удаления существующего лимита {limitId} сервер Газпром вернул ответ что лимит не удален.";

				LogErrorMessageAndThrowException(errorMessage);
			}

			return true;
		}

		private HttpContent CreateRemoveLimitHttpContent(string contractId, string limitId, string apiKey, string sessionId)
		{
			var requestData = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("contract_id", contractId),
				new KeyValuePair<string, string>("limit_id", limitId)
			};

			var content = new FormUrlEncodedContent(requestData);
			content.Headers.Add("api_key", apiKey);
			content.Headers.Add("session_id", sessionId);
			content.Headers.Add("date_time", DateTime.Now.ToString(_requestDateTimeFormatString));

			return content;
		}

		public async Task<IEnumerable<string>> SetFuelLimit(
			FuelLimit fuelLimit,
			string sessionId,
			string apiKey,
			CancellationToken cancellationToken)
		{
			if(fuelLimit is null)
			{
				throw new ArgumentNullException(nameof(fuelLimit));
			}

			if(string.IsNullOrWhiteSpace(sessionId))
			{
				throw new ArgumentException($"'{nameof(sessionId)}' cannot be null or whitespace.", nameof(sessionId));
			}

			if(string.IsNullOrWhiteSpace(apiKey))
			{
				throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or whitespace.", nameof(apiKey));
			}

			var requestDto = _fuelLimitConverter.ConvertFuelLimitToRequestDto(fuelLimit, _fuelControlSettings.LiterUnitId, _fuelControlSettings.RubleCurrencyId);
			var requestParameters = JsonSerializer.Serialize(requestDto);

			var httpContent = CreateSetLimitHttpContent(requestParameters, apiKey, sessionId);

			_logger.LogDebug("Выполняется создания нового лимита. Параметры запроса: {RequestParameters}, ключ API {ApiKey}",
				requestParameters,
				apiKey);

			var httpClient = _httpClientFactory.CreateClient(GazpromHttpClientNames.Default);
			var response = await httpClient.PostAsync(_setLimitEndpointAddress, httpContent, cancellationToken);

			var responseString = await response.Content.ReadAsStringAsync();

			var responseData = JsonSerializer.Deserialize<SetFuelLimitResponse>(responseString);

			if(responseData.Status.Errors?.Count() > 0)
			{
				var errorMessage =
					$"На запрос создания нового лимита сервер Газпром вернул ответ с ошибками: {string.Concat(responseData.Status.Errors.Select(e => $"\nТип: {e.ErrorType}. Сообщение: {e.Message}"))}";

				LogErrorMessageAndThrowException(errorMessage);
			}

			if(responseData.CreatedLimitsIds?.Count() < 1)
			{
				var errorMessage =
					$"Ответ на запрос создания нового лимита не содержит Id созданных лимитов.";

				LogErrorMessageAndThrowException(errorMessage);
			}

			return responseData.CreatedLimitsIds;
		}

		private HttpContent CreateSetLimitHttpContent(string requestParameters, string apiKey, string sessionId)
		{
			var requestData = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("limit", $"[{requestParameters}]")
			};

			var content = new FormUrlEncodedContent(requestData);
			content.Headers.Add("api_key", apiKey);
			content.Headers.Add("session_id", sessionId);
			content.Headers.Add("date_time", DateTime.Now.ToString(_requestDateTimeFormatString));

			return content;
		}

		private void LogErrorMessageAndThrowException(string errorMessage)
		{
			_logger.LogError(errorMessage);

			throw new FuelControlException(errorMessage);
		}
	}
}
