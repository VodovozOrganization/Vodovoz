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
	public class PlannedOrdersNotificationsSendClient : IPlannedOrdersNotificationsSendClient
	{
		private readonly HttpClient _httpClient;
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;

		public PlannedOrdersNotificationsSendClient(
			HttpClient httpClient,
			IBitrixNotificationsSendSettings bitrixNotificationsSendSettings)
		{
			_httpClient = httpClient
				?? throw new ArgumentNullException(nameof(httpClient));
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings
				?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
		}

		public async Task<Result> CreatePlannedOrderDeal(PlannedOrderDto plannedOrder, CancellationToken cancellationToken)
		{
			var request = new CreatePlannedOrderDealRequest
			{
				Fields = plannedOrder
			};

			var content = JsonSerializer.Serialize(request);

			return await CreateDeal(content, cancellationToken);
		}

		public async Task<Result<PlannedOrderDealsBatchResult>> CreatePlannedOrderDeals(
			IEnumerable<PlannedOrderDto> plannedOrders,
			CancellationToken cancellationToken)
		{
			var commands = new Dictionary<string, string>();

			foreach(var plannedOrder in plannedOrders)
			{
				commands.Add(plannedOrder.DealCommandKey, CreateDealAddCommand(plannedOrder));
			}

			if(commands.Count == 0)
			{
				return new PlannedOrderDealsBatchResult();
			}

			if(commands.Count > CreateDealsBatchRequest.MaxCommandsCount)
			{
				throw new ArgumentException(
					$"Количество сделок в пакете не должно превышать {CreateDealsBatchRequest.MaxCommandsCount}",
					nameof(plannedOrders));
			}

			var request = new CreateDealsBatchRequest
			{
				Commands = commands
			};

			var content = JsonSerializer.Serialize(request);

			return await CreateDealsBatch(content, cancellationToken);
		}

		private async Task<Result> CreateDeal(string content, CancellationToken cancellationToken)
		{
			var retryPolicy = CreateRetryPolicy(cancellationToken);

			var result = await retryPolicy.ExecuteAndCaptureAsync(
				async (innerCancellationToken) =>
				{
					var httpContent = new StringContent(content, Encoding.UTF8, "application/json");

					var response = await _httpClient.PostAsync(
						$"rest/{_bitrixNotificationsSendSettings.BitrixDealsUser}/{_bitrixNotificationsSendSettings.BitrixDealsToken}/crm.deal.add",
						httpContent,
						innerCancellationToken);

					var responseResult =
						response.IsSuccessStatusCode
						? Result.Success()
						: Result.Failure(Errors.BitrixNotificationsSendErrors.CreateSendPlannedOrdersNotificationError(response.ReasonPhrase));
					return responseResult;
				},
				cancellationToken);

			return result.Result
				?? Errors.BitrixNotificationsSendErrors.CreateSendPlannedOrdersNotificationError(result.FinalException.Message);
		}

		private async Task<Result<PlannedOrderDealsBatchResult>> CreateDealsBatch(string content, CancellationToken cancellationToken)
		{
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
						return Result.Failure<PlannedOrderDealsBatchResult>(
							Errors.BitrixNotificationsSendErrors.CreateSendPlannedOrdersNotificationError(response.ReasonPhrase));
					}

					var responseBody = await response.Content.ReadAsStringAsync();

					return ParseDealsBatchResponse(responseBody);
				},
				cancellationToken);

			return result.Result
				?? Result.Failure<PlannedOrderDealsBatchResult>(
					Errors.BitrixNotificationsSendErrors.CreateSendPlannedOrdersNotificationError(result.FinalException.Message));
		}

		private static Result<PlannedOrderDealsBatchResult> ParseDealsBatchResponse(string responseBody)
		{
			CreateDealsBatchResponse batchResponse;

			try
			{
				batchResponse = JsonSerializer.Deserialize<CreateDealsBatchResponse>(responseBody);
			}
			catch(JsonException ex)
			{
				return Result.Failure<PlannedOrderDealsBatchResult>(
					Errors.BitrixNotificationsSendErrors.CreateSendPlannedOrdersNotificationError(
						$"Не удалось разобрать ответ пакетного запроса: {ex.Message}"));
			}

			if(batchResponse?.Result == null)
			{
				return Result.Failure<PlannedOrderDealsBatchResult>(
					Errors.BitrixNotificationsSendErrors.CreateSendPlannedOrdersNotificationError(
						"Ответ пакетного запроса не содержит результата"));
			}

			var batchResult = new PlannedOrderDealsBatchResult();

			foreach(var createdDeal in batchResponse.Result.CreatedDeals)
			{
				batchResult.CreatedDealKeys.Add(createdDeal.Key);
			}

			foreach(var commandError in batchResponse.Result.Errors)
			{
				batchResult.Errors.Add(new PlannedOrderDealCreationError
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
			IEnumerable<CreateDealsBatchCommandTime> commandsTime,
			PlannedOrderDealsBatchResult batchResult)
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

		private static string CreateDealAddCommand(PlannedOrderDto plannedOrder)
		{
			using(var document = JsonDocument.Parse(JsonSerializer.Serialize(plannedOrder)))
			{
				var parameters = new List<string>();

				foreach(var property in document.RootElement.EnumerateObject())
				{
					if(property.Value.ValueKind == JsonValueKind.Null)
					{
						continue;
					}

					var value = property.Value.ValueKind == JsonValueKind.String
						? property.Value.GetString()
						: property.Value.GetRawText();

					parameters.Add($"fields[{property.Name}]={Uri.EscapeDataString(value)}");
				}

				return $"crm.deal.add?{string.Join("&", parameters)}";
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
