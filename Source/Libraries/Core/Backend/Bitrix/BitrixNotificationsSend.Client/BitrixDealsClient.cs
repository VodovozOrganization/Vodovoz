using BitrixNotificationsSend.Contracts;
using BitrixNotificationsSend.Contracts.Dto;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Client
{
	public class BitrixDealsClient : IBitrixDealsClient
	{
		private const string _dealAddMethod = "crm.deal.add";
		private const string _dealUpdateMethod = "crm.deal.update";
		private const string _dealGetMethod = "crm.deal.get";
		private const string _dealStageFieldName = "STAGE_ID";
		private const string _dealStageCommandKeyPrefix = "deal_stage_";
		private const string _notFoundMessage = "not found";

		private static readonly string[] _undeliveredOrderDealUpdateExcludedFields = { "CATEGORY_ID", "STAGE_ID" };

		private readonly HttpClient _httpClient;
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;

		public BitrixDealsClient(
			HttpClient httpClient,
			IBitrixNotificationsSendSettings bitrixNotificationsSendSettings)
		{
			_httpClient = httpClient
				?? throw new ArgumentNullException(nameof(httpClient));
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings
				?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
		}

		public async Task<Result<BitrixBatchResult>> SendPlannedOrderDeals(
			IEnumerable<PlannedOrderDto> plannedOrders,
			CancellationToken cancellationToken)
		{
			var commands = new Dictionary<string, string>();

			foreach(var plannedOrder in plannedOrders)
			{
				commands.Add(plannedOrder.DealCommandKey, BitrixCommandBuilder.CreateCommand(_dealAddMethod, plannedOrder));
			}

			return await SendBatch(commands, cancellationToken);
		}

		public async Task<Result<BitrixBatchResult>> UpdatePlannedOrderDeals(
			IEnumerable<PlannedOrderDealUpdateDto> plannedOrderDealUpdates,
			CancellationToken cancellationToken)
		{
			var commands = new Dictionary<string, string>();

			foreach(var plannedOrderDealUpdate in plannedOrderDealUpdates)
			{
				commands.Add(
					plannedOrderDealUpdate.DealCommandKey,
					BitrixCommandBuilder.CreateCommand(
						_dealUpdateMethod,
						plannedOrderDealUpdate,
						$"id={plannedOrderDealUpdate.BitrixDealId}"));
			}

			return await SendBatch(commands, cancellationToken);
		}

		public async Task<Result<BitrixDealsStagesResult>> GetDealsStages(
			IEnumerable<long> dealIds,
			CancellationToken cancellationToken)
		{
			var commands = new Dictionary<string, string>();

			foreach(var dealId in dealIds)
			{
				commands.Add($"{_dealStageCommandKeyPrefix}{dealId}", $"{_dealGetMethod}?id={dealId}");
			}

			if(commands.Count == 0)
			{
				return new BitrixDealsStagesResult();
			}

			var batchResponse = await SendBatchRequest(commands, cancellationToken);

			if(batchResponse.IsFailure)
			{
				return Result.Failure<BitrixDealsStagesResult>(batchResponse.Errors);
			}

			return ParseDealsStages(batchResponse.Value);
		}

		public async Task<Result<BitrixBatchResult>> LastServiceOrderDeals(
			IEnumerable<LastServiceOrderDto> lastServiceOrders,
			CancellationToken cancellationToken)
		{
			var commands = new Dictionary<string, string>();

			foreach(var lastServiceOrder in lastServiceOrders)
			{
				commands.Add(lastServiceOrder.DealCommandKey, BitrixCommandBuilder.CreateCommand(_dealAddMethod, lastServiceOrder));
			}

			return await SendBatch(commands, cancellationToken);
		}

		public async Task<Result<BitrixBatchResult>> SendUndeliveredOrderDeals(
			IEnumerable<UndeliveredOrderDto> undeliveredOrders,
			CancellationToken cancellationToken)
		{
			var commands = new Dictionary<string, string>();

			foreach(var undeliveredOrder in undeliveredOrders)
			{
				commands.Add(undeliveredOrder.DealCommandKey, BitrixCommandBuilder.CreateCommand(_dealAddMethod, undeliveredOrder));
			}

			return await SendBatch(commands, cancellationToken);
		}

		public async Task<Result<BitrixBatchResult>> UpdateUndeliveredOrderDeals(
			IEnumerable<UndeliveredOrderDto> undeliveredOrders,
			CancellationToken cancellationToken)
		{
			var commands = new Dictionary<string, string>();

			foreach(var undeliveredOrder in undeliveredOrders)
			{
				commands.Add(
					undeliveredOrder.DealCommandKey,
					BitrixCommandBuilder.CreateCommand(
						_dealUpdateMethod,
						undeliveredOrder,
						_undeliveredOrderDealUpdateExcludedFields,
						$"id={undeliveredOrder.BitrixDealId.Value}"));
			}

			return await SendBatch(commands, cancellationToken);
		}

		private async Task<Result<BitrixBatchResult>> SendBatch(
			IDictionary<string, string> commands,
			CancellationToken cancellationToken)
		{
			if(commands is null || commands.Count == 0)
			{
				return new BitrixBatchResult();
			}

			var batchResponse = await SendBatchRequest(commands, cancellationToken);

			if(batchResponse.IsFailure)
			{
				return Result.Failure<BitrixBatchResult>(batchResponse.Errors);
			}

			return CreateBatchResult(batchResponse.Value);
		}

		/// <summary>
		/// Отправка пакетного запроса batch.json в Битрикс24 с повторами при сетевых ошибках
		/// </summary>
		/// <param name="commands">Команды пакета: ключ команды - строка вызова метода</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Разобранный ответ Битрикс24 на пакетный запрос</returns>
		private async Task<Result<BitrixBatchResponse>> SendBatchRequest(
			IDictionary<string, string> commands,
			CancellationToken cancellationToken)
		{
			if(commands.Count > BitrixApiLimits.MaxBatchCommandsCount)
			{
				throw new ArgumentException(
					$"Количество команд в пакете не должно превышать {BitrixApiLimits.MaxBatchCommandsCount}",
					nameof(commands));
			}

			var request = new BitrixBatchRequest
			{
				Commands = commands
			};

			var content = JsonSerializer.Serialize(request);

			var retryPolicy = CreateRetryPolicy(cancellationToken);

			var result = await retryPolicy.ExecuteAndCaptureAsync(
				async (innerCancellationToken) =>
				{
					var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

					var response = await _httpClient.PostAsync(
						$"rest/{_bitrixNotificationsSendSettings.BitrixDealsUser}/{_bitrixNotificationsSendSettings.BitrixDealsToken}/batch.json",
						httpContent,
						innerCancellationToken);

					if(!response.IsSuccessStatusCode)
					{
						return Result.Failure<BitrixBatchResponse>(
							Errors.BitrixNotificationsSendErrors.CreateBatchRequestError(response.ReasonPhrase));
					}

					var responseBody = await response.Content.ReadAsStringAsync();

					return ParseBatchResponse(responseBody);
				},
				cancellationToken);

			return result.Result
				?? Result.Failure<BitrixBatchResponse>(
					Errors.BitrixNotificationsSendErrors.CreateBatchRequestError(result.FinalException.Message));
		}

		private static Result<BitrixBatchResponse> ParseBatchResponse(string responseBody)
		{
			BitrixBatchResponse batchResponse;

			try
			{
				batchResponse = JsonSerializer.Deserialize<BitrixBatchResponse>(responseBody);
			}
			catch(JsonException ex)
			{
				return Result.Failure<BitrixBatchResponse>(
					Errors.BitrixNotificationsSendErrors.CreateBatchRequestError(
						$"Не удалось разобрать ответ пакетного запроса: {ex.Message}"));
			}

			if(batchResponse?.Result == null)
			{
				return Result.Failure<BitrixBatchResponse>(
					Errors.BitrixNotificationsSendErrors.CreateBatchRequestError(
						"Ответ пакетного запроса не содержит результата"));
			}

			return batchResponse;
		}

		private static BitrixBatchResult CreateBatchResult(BitrixBatchResponse batchResponse)
		{
			var batchResult = new BitrixBatchResult();

			foreach(var successfulCommand in batchResponse.Result.SuccessfulCommands)
			{
				batchResult.SuccessfulCommandKeys.Add(successfulCommand.Key);

				if(TryGetCreatedEntityId(successfulCommand.Value, out var entityId))
				{
					batchResult.SuccessfulCommandEntityIds.Add(successfulCommand.Key, entityId);
				}
			}

			foreach(var commandError in batchResponse.Result.Errors)
			{
				batchResult.Errors.Add(new BitrixBatchItemError
				{
					CommandKey = commandError.Key,
					ErrorCode = commandError.Value?.Error,
					Message = commandError.Value?.ErrorDescription
				});
			}

			FillOperatingData(batchResponse.Result.CommandsTime.Values, batchResult);

			return batchResult;
		}

		/// <summary>
		/// Разбор ответа пакетного запроса чтения сделок.
		/// Сделки, по которым Битрикс24 вернул ошибку отсутствия сделки, считаются удалёнными,
		/// остальные ошибки возвращаются для повторной обработки
		/// </summary>
		/// <param name="batchResponse">Ответ Битрикс24 на пакетный запрос batch.json</param>
		/// <returns>Результат чтения текущих стадий сделок из Битрикс24</returns>
		private static BitrixDealsStagesResult ParseDealsStages(BitrixBatchResponse batchResponse)
		{
			var stagesResult = new BitrixDealsStagesResult();

			foreach(var successfulCommand in batchResponse.Result.SuccessfulCommands)
			{
				if(!TryGetDealIdFromCommandKey(successfulCommand.Key, out var dealId))
				{
					continue;
				}

				if(successfulCommand.Value.ValueKind == JsonValueKind.Object
					&& successfulCommand.Value.TryGetProperty(_dealStageFieldName, out var stageValue)
					&& stageValue.ValueKind == JsonValueKind.String)
				{
					stagesResult.StagesByDealIds[dealId] = stageValue.GetString();
				}
			}

			foreach(var commandError in batchResponse.Result.Errors)
			{
				if(!TryGetDealIdFromCommandKey(commandError.Key, out var dealId))
				{
					continue;
				}

				var itemError = new BitrixBatchItemError
				{
					CommandKey = commandError.Key,
					ErrorCode = commandError.Value?.Error,
					Message = commandError.Value?.ErrorDescription
				};

				if(IsDealNotFoundError(itemError))
				{
					stagesResult.NotFoundDealIds.Add(dealId);
					continue;
				}

				stagesResult.Errors.Add(itemError);
			}

			return stagesResult;
		}

		private static bool TryGetDealIdFromCommandKey(string commandKey, out long dealId)
		{
			dealId = default;

			if(string.IsNullOrWhiteSpace(commandKey)
				|| !commandKey.StartsWith(_dealStageCommandKeyPrefix, StringComparison.Ordinal))
			{
				return false;
			}

			return long.TryParse(commandKey.Substring(_dealStageCommandKeyPrefix.Length), out dealId);
		}

		private static bool IsDealNotFoundError(BitrixBatchItemError itemError)
		{
			if(itemError.IsOperatingLimitError)
			{
				return false;
			}

			var isNotFound =
				!string.IsNullOrWhiteSpace(itemError.Message)
				&& (itemError.Message.IndexOf(_notFoundMessage, StringComparison.OrdinalIgnoreCase) >= 0);

			return isNotFound;
		}

		private static bool TryGetCreatedEntityId(JsonElement value, out long entityId)
		{
			switch(value.ValueKind)
			{
				case JsonValueKind.Number:
					return value.TryGetInt64(out entityId);
				case JsonValueKind.String:
					return long.TryParse(value.GetString(), out entityId);
				default:
					entityId = default;
					return false;
			}
		}

		private static void FillOperatingData(
			IEnumerable<BitrixBatchCommandTime> commandsTime,
			BitrixBatchResult batchResult)
		{
			foreach(var commandTime in commandsTime)
			{
				if(commandTime == null)
				{
					continue;
				}

				if(commandTime.Operating > batchResult.OperatingSeconds)
				{
					batchResult.OperatingSeconds = commandTime.Operating;
				}

				if(commandTime.OperatingResetAtUtc != null
					&& (batchResult.OperatingResetAt == null || commandTime.OperatingResetAtUtc > batchResult.OperatingResetAt))
				{
					batchResult.OperatingResetAt = commandTime.OperatingResetAtUtc;
				}
			}
		}

		private static AsyncRetryPolicy CreateRetryPolicy(CancellationToken cancellationToken) =>
			Policy
				.Handle<HttpRequestException>()
				.Or<TimeoutException>()
				.Or<TaskCanceledException>(ex => !cancellationToken.IsCancellationRequested)
				.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
	}
}
