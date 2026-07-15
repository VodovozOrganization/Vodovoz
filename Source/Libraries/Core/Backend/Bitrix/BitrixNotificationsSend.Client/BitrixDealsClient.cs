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

		/// <summary>
		/// Отправка пакета команд одним запросом batch.json,
		/// не более <see cref="BitrixApiLimits.MaxBatchCommandsCount"/> команд за один вызов
		/// </summary>
		private async Task<Result<BitrixBatchResult>> SendBatch(
			IDictionary<string, string> commands,
			CancellationToken cancellationToken)
		{
			if(commands is null || commands.Count == 0)
			{
				return new BitrixBatchResult();
			}

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
						return Result.Failure<BitrixBatchResult>(
							Errors.BitrixNotificationsSendErrors.CreateBatchRequestError(response.ReasonPhrase));
					}

					var responseBody = await response.Content.ReadAsStringAsync();

					return ParseBatchResponse(responseBody);
				},
				cancellationToken);

			return result.Result
				?? Result.Failure<BitrixBatchResult>(
					Errors.BitrixNotificationsSendErrors.CreateBatchRequestError(result.FinalException.Message));
		}

		private static Result<BitrixBatchResult> ParseBatchResponse(string responseBody)
		{
			BitrixBatchResponse batchResponse;

			try
			{
				batchResponse = JsonSerializer.Deserialize<BitrixBatchResponse>(responseBody);
			}
			catch(JsonException ex)
			{
				return Result.Failure<BitrixBatchResult>(
					Errors.BitrixNotificationsSendErrors.CreateBatchRequestError(
						$"Не удалось разобрать ответ пакетного запроса: {ex.Message}"));
			}

			if(batchResponse?.Result == null)
			{
				return Result.Failure<BitrixBatchResult>(
					Errors.BitrixNotificationsSendErrors.CreateBatchRequestError(
						"Ответ пакетного запроса не содержит результата"));
			}

			var batchResult = new BitrixBatchResult();

			foreach(var successfulCommand in batchResponse.Result.SuccessfulCommands)
			{
				batchResult.SuccessfulCommandKeys.Add(successfulCommand.Key);
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
