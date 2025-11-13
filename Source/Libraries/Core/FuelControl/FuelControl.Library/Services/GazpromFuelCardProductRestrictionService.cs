using FuelControl.Contracts.Dto;
using FuelControl.Contracts.Responses;
using FuelControl.Library.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Fuel;

namespace FuelControl.Library.Services
{
	public class GazpromFuelCardProductRestrictionService : IFuelControlFuelCardProductRestrictionService
	{
		private const string _requestDateTimeFormatString = "yyyy-MM-dd HH:mm:ss";
		private const string _cardsEndpointAddress = "vip/v1/restriction";
		private const string _removeRestrictionEndpointAddress = "vip/v1/removeRestriction";
		private const string _setRestrictionEndpointAddress = "vip/v1/setRestriction";

		private readonly ILogger<GazpromFuelCardProductRestrictionService> _logger;
		private readonly IFuelControlSettings _fuelControlSettings;

		public GazpromFuelCardProductRestrictionService(
			ILogger<GazpromFuelCardProductRestrictionService> logger,
			IFuelControlSettings fuelControlSettings)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
		}

		public async Task<IEnumerable<string>> GetProductRestrictionsByCardId(
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
				"Запрос на получение товарных ограничений по карте. 'CardId'={PageLimit}",
				cardId);

			var baseAddress = new Uri(_fuelControlSettings.ApiBaseAddress);

			using(var httpClient = new HttpClient { BaseAddress = baseAddress })
			{
				httpClient.Timeout = TimeSpan.FromSeconds(_fuelControlSettings.ApiRequesTimeout.TotalSeconds);
				httpClient.DefaultRequestHeaders.Add("api_key", apiKey);
				httpClient.DefaultRequestHeaders.Add("session_id", sessionId);
				httpClient.DefaultRequestHeaders.Add("date_time", DateTime.Now.ToString(_requestDateTimeFormatString));

				var response = await httpClient.GetAsync(
					  $"{_cardsEndpointAddress}?contract_id={_fuelControlSettings.OrganizationContractId}&card_id={cardId}",
					  cancellationToken);

				var responseString = await response.Content.ReadAsStringAsync();

				var responseData = JsonSerializer.Deserialize<FuelCardProductRestrictionsResponse>(responseString);

				if(responseData.Status.Errors?.Count() > 0)
				{
					var errorMessage =
						$"На запрос получения списка товарных ограничителей сервер Газпром вернул ответ с ошибками: " +
						$"{string.Concat(responseData.Status.Errors.Select(e => $"Тип: {e.ErrorType}. Сообщение: {e.Message}"))}";

					LogErrorMessageAndThrowException(errorMessage);
				}

				_logger.LogDebug("Количество полученных товарных ограничителей: {RestrictionsCount}",
					responseData.FuelCardProductRestrictionsData.FuelCardProductRestrictionsCount);

				var restrictions = responseData.FuelCardProductRestrictionsData.FuelCardProductRestrictions;

				return restrictions.Select(x => x.Id).ToList();
			}
		}

		public async Task<bool> RemoveProductRestictionById(
			string restrictionId,
			string sessionId,
			string apiKey,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(restrictionId))
			{
				throw new ArgumentException($"'{nameof(restrictionId)}' cannot be null or whitespace.", nameof(restrictionId));
			}

			if(string.IsNullOrWhiteSpace(sessionId))
			{
				throw new ArgumentException($"'{nameof(sessionId)}' cannot be null or whitespace.", nameof(sessionId));
			}

			if(string.IsNullOrWhiteSpace(apiKey))
			{
				throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or whitespace.", nameof(apiKey));
			}

			var baseAddress = new Uri(_fuelControlSettings.ApiBaseAddress);
			var httpContent = CreateRemoveRestrictionHttpContent(_fuelControlSettings.OrganizationContractId, restrictionId, apiKey, sessionId);

			_logger.LogDebug("Выполняется запрос удаления существующего товарного ограничителя {RestrictionId}. Id сессии {SessionId}, ключ API {ApiKey}",
				restrictionId,
				sessionId,
				apiKey);

			using(var httpClient = new HttpClient { BaseAddress = baseAddress })
			{
				var response = await httpClient.PostAsync(_removeRestrictionEndpointAddress, httpContent, cancellationToken);

				var responseString = await response.Content.ReadAsStringAsync();

				var responseData = JsonSerializer.Deserialize<RemoveFuelLimitResponse>(responseString);

				if(responseData.Status.Errors?.Count() > 0)
				{
					var errorMessage =
						$"На запрос удаления существующего товарного ограничителя {restrictionId} сервер Газпром вернул ответ с ошибками: " +
						$"{string.Concat(responseData.Status.Errors.Select(e => $"\nТип: {e.ErrorType}. Сообщение: {e.Message}"))}";

					LogErrorMessageAndThrowException(errorMessage);
				}

				if(!responseData.IsRemovalSuccessful)
				{
					var errorMessage =
						$"На запрос удаления существующего товарного ограничителя {restrictionId} сервер Газпром вернул ответ что ограничитель не удален.";

					LogErrorMessageAndThrowException(errorMessage);
				}

				return true;
			}
		}

		public async Task<IEnumerable<long>> SetCommonFuelRestriction(
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

			return await SetProductRestriction(cardId, string.Empty, sessionId, apiKey, cancellationToken);
		}

		public async Task<IEnumerable<long>> SetFuelProductGroupRestriction(
			string cardId,
			string productGroupId,
			string sessionId,
			string apiKey,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(cardId))
			{
				throw new ArgumentException($"'{nameof(cardId)}' cannot be null or whitespace.", nameof(cardId));
			}

			if(string.IsNullOrWhiteSpace(productGroupId))
			{
				throw new ArgumentException($"'{nameof(productGroupId)}' cannot be null or whitespace.", nameof(productGroupId));
			}

			if(string.IsNullOrWhiteSpace(sessionId))
			{
				throw new ArgumentException($"'{nameof(sessionId)}' cannot be null or whitespace.", nameof(sessionId));
			}

			if(string.IsNullOrWhiteSpace(apiKey))
			{
				throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or whitespace.", nameof(apiKey));
			}

			return await SetProductRestriction(cardId, productGroupId, sessionId, apiKey, cancellationToken);
		}

		private async Task<IEnumerable<long>> SetProductRestriction(
			string cardId,
			string productGroupId,
			string sessionId,
			string apiKey,
			CancellationToken cancellationToken)
		{
			var baseAddress = new Uri(_fuelControlSettings.ApiBaseAddress);
			var httpContent =
				CreateSetRestrictionHttpContent(_fuelControlSettings.OrganizationContractId, cardId, productGroupId, apiKey, sessionId);

			_logger.LogDebug(
				"Выполняется создания нового товарного ограничителя." +
				" CardId: {CardId}, ProductGroupId: {ProductGroupId}, Sessionid: {SessionId} , ApiKey {ApiKey}",
				cardId,
				productGroupId,
				sessionId,
				apiKey);

			using(var httpClient = new HttpClient { BaseAddress = baseAddress })
			{
				var response = await httpClient.PostAsync(_setRestrictionEndpointAddress, httpContent, cancellationToken);

				var responseString = await response.Content.ReadAsStringAsync();

				var responseData = JsonSerializer.Deserialize<SetFuelCardProductRestrictionResponse>(responseString);

				if(responseData.Status.Errors?.Count() > 0)
				{
					var errorMessage =
						$"На запрос создания нового товарного ограничителя сервер Газпром вернул ответ с ошибками: " +
						$"{string.Concat(responseData.Status.Errors.Select(e => $"\nТип: {e.ErrorType}. Сообщение: {e.Message}"))}";

					LogErrorMessageAndThrowException(errorMessage);
				}

				if(responseData.CreatedRestrictionsIds?.Count() < 1)
				{
					var errorMessage =
						$"Ответ на запрос создания нового товарного ограничителя не содержит Id созданных ограничителей.";

					LogErrorMessageAndThrowException(errorMessage);
				}

				return responseData.CreatedRestrictionsIds.ToList();
			}
		}

		private HttpContent CreateRemoveRestrictionHttpContent(string contractId, string restrictionId, string apiKey, string sessionId)
		{
			var requestData = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("contract_id", contractId),
				new KeyValuePair<string, string>("restriction_id", restrictionId)
			};

			var content = new FormUrlEncodedContent(requestData);
			content.Headers.Add("api_key", apiKey);
			content.Headers.Add("session_id", sessionId);
			content.Headers.Add("date_time", DateTime.Now.ToString(_requestDateTimeFormatString));

			return content;
		}

		private HttpContent CreateSetRestrictionHttpContent(string contractId, string cardId, string productGroupId, string apiKey, string sessionId)
		{
			var restriction = new FuelCardProductRestrictionDto
			{
				ContractId = contractId,
				CardId = cardId,
				ProductTypeId = _fuelControlSettings.FuelProductTypeId,
				RestrictionType = 1
			};

			if(!string.IsNullOrWhiteSpace(productGroupId))
			{
				restriction.ProductGroupId = productGroupId;
			}

			var requestParameters = JsonSerializer.Serialize(restriction);

			var requestData = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("restriction", $"[{requestParameters}]")
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
