using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxAuthorizedLoyaltyOrderService : IMindboxAuthorizedLoyaltyOrderService
	{
		private const string SuccessStatus = "Success";
		private const string TransactionAlreadyProcessedStatus = "TransactionAlreadyProcessed";
		private const string CreatedStatus = "Created";
		private const string ProcessedStatus = "Processed";
		private const string UpdatedStatus = "Updated";
		private const string PriceHasBeenChangedStatus = "PriceHasBeenChanged";
		private const string BonusPointsLocation = "order/bonusPoints";

		private readonly IMindboxClient _client;
		private readonly IMindboxSettings _settings;
		private readonly IMindboxAlertSender _alertSender;
		private readonly ILogger<MindboxAuthorizedLoyaltyOrderService> _logger;

		public MindboxAuthorizedLoyaltyOrderService(
			IMindboxClient client,
			IMindboxSettings settings,
			IMindboxAlertSender alertSender,
			ILogger<MindboxAuthorizedLoyaltyOrderService> logger)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_alertSender = alertSender ?? throw new ArgumentNullException(nameof(alertSender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task<MindboxLoyaltyOrderResult> CreateAsync(
			MindboxLoyaltyOrderRequest request,
			int onlineOrderId,
			Guid externalOrderId,
			int erpCounterpartyId,
			Guid transactionId,
			CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var requestedAmount = request.Order?.BonusPoints?.FirstOrDefault()?.Amount ?? 0;
			var operation = await _client
				.CreateAuthorizedLoyaltyOrderAsync(request, transactionId, cancellationToken)
				.ConfigureAwait(false);

			if(operation.Outcome == MindboxSendOutcome.TransientError)
			{
				await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);

				operation = await _client
					.CreateAuthorizedLoyaltyOrderAsync(request, transactionId, cancellationToken)
					.ConfigureAwait(false);
			}

			var details = string.Format(
				"onlineOrderId={0}, externalOrderId={1}, erpCounterpartyId={2}, transactionId={3}, requested={4}, httpCode={5}, status={6}, processingStatus={7}, errorId={8}, durationMs={9}, outcome={10}",
				onlineOrderId,
				externalOrderId,
				erpCounterpartyId,
				transactionId,
				requestedAmount,
				operation.HttpStatusCode,
				operation.Status,
				null,
				operation.ErrorId,
				operation.DurationMs,
				operation.Outcome);

			if(operation.Outcome == MindboxSendOutcome.ValidationError)
			{
				if(IsBonusAmountValidationError(operation.RawResponse))
				{
					_logger.LogWarning("CreateAuthorizedLoyaltyOrder: бонусы изменились. {Details}", details);

					return MindboxLoyaltyOrderResult.Failure(
						MindboxLoyaltyOrderOutcome.BonusesChanged,
						operation.ErrorMessage,
						operation.HttpStatusCode,
						operation.Status,
						null,
						operation.ErrorId,
						operation.DurationMs);
				}

				_logger.LogError("CreateAuthorizedLoyaltyOrder: ошибка данных. {Details}", details);

				await SendAlertAsync("Mindbox CreateAuthorizedLoyaltyOrder: ошибка данных запроса", cancellationToken)
					.ConfigureAwait(false);

				return MindboxLoyaltyOrderResult.Failure(
					MindboxLoyaltyOrderOutcome.ServiceUnavailable,
					operation.ErrorMessage,
					operation.HttpStatusCode,
					operation.Status,
					null,
					operation.ErrorId,
					operation.DurationMs);
			}

			if(!operation.IsSuccess)
			{
				_logger.LogError("CreateAuthorizedLoyaltyOrder: ошибка. {Details}", details);

				await SendAlertAsync(
					$"Mindbox CreateAuthorizedLoyaltyOrder: ошибка, HTTP {operation.HttpStatusCode}, status {operation.Status}",
					cancellationToken)
					.ConfigureAwait(false);

				return MindboxLoyaltyOrderResult.Failure(
					MindboxLoyaltyOrderOutcome.ServiceUnavailable,
					operation.ErrorMessage,
					operation.HttpStatusCode,
					operation.Status,
					null,
					operation.ErrorId,
					operation.DurationMs);
			}

			if(string.Equals(operation.Status, TransactionAlreadyProcessedStatus, StringComparison.OrdinalIgnoreCase))
			{
				_logger.LogInformation("CreateAuthorizedLoyaltyOrder: транзакция уже обработана. {Details}", details);

				return MindboxLoyaltyOrderResult.Success(
					null,
					requestedAmount,
					null,
					operation.HttpStatusCode,
					operation.Status,
					null,
					operation.ErrorId,
					operation.DurationMs);
			}

			MindboxLoyaltyOrderResponse response;

			try
			{
				response = JsonSerializer.Deserialize<MindboxLoyaltyOrderResponse>(operation.RawResponse);
			}
			catch(JsonException ex)
			{
				_logger.LogError(ex, "CreateAuthorizedLoyaltyOrder: не удалось разобрать ответ. {Details}", details);

				return MindboxLoyaltyOrderResult.Failure(
					MindboxLoyaltyOrderOutcome.ServiceUnavailable,
					ex.Message,
					operation.HttpStatusCode,
					operation.Status,
					null,
					operation.ErrorId,
					operation.DurationMs);
			}

			if(response?.Order is null)
			{
				return MindboxLoyaltyOrderResult.Failure(
					MindboxLoyaltyOrderOutcome.ServiceUnavailable,
					"В ответе CreateAuthorizedLoyaltyOrder нет order",
					operation.HttpStatusCode,
					response?.Status,
					null,
					operation.ErrorId,
					operation.DurationMs);
			}

			if(!string.Equals(response.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase))
			{
				return MindboxLoyaltyOrderResult.Failure(
					MindboxLoyaltyOrderOutcome.ServiceUnavailable,
					$"Статус CreateAuthorizedLoyaltyOrder: {response.Status}",
					operation.HttpStatusCode,
					response.Status,
					response.Order.ProcessingStatus,
					operation.ErrorId,
					operation.DurationMs);
			}

			if(string.Equals(response.Order.ProcessingStatus, PriceHasBeenChangedStatus, StringComparison.OrdinalIgnoreCase))
			{
				_logger.LogWarning("CreateAuthorizedLoyaltyOrder: цена изменилась. {Details}", details);

				return MindboxLoyaltyOrderResult.Failure(
					MindboxLoyaltyOrderOutcome.BonusesChanged,
					"PriceHasBeenChanged",
					operation.HttpStatusCode,
					operation.Status,
					response.Order.ProcessingStatus,
					operation.ErrorId,
					operation.DurationMs);
			}

			if(!string.Equals(response.Order.ProcessingStatus, CreatedStatus, StringComparison.OrdinalIgnoreCase)
				&& !string.Equals(response.Order.ProcessingStatus, ProcessedStatus, StringComparison.OrdinalIgnoreCase)
				&& !string.Equals(response.Order.ProcessingStatus, UpdatedStatus, StringComparison.OrdinalIgnoreCase))
			{
				return MindboxLoyaltyOrderResult.Failure(
					MindboxLoyaltyOrderOutcome.ServiceUnavailable,
					$"Неожиданный processingStatus: {response.Order.ProcessingStatus}",
					operation.HttpStatusCode,
					operation.Status,
					response.Order.ProcessingStatus,
					operation.ErrorId,
					operation.DurationMs);
			}

			var change = GetBonusChange(response.Order.BonusPointsChanges);

			if(change is null || ToBonusPoints(change.SpentAmount) != requestedAmount)
			{
				_logger.LogWarning(
					"CreateAuthorizedLoyaltyOrder: списано {Spent}, запрошено {Requested}. {Details}",
					change?.SpentAmount,
					requestedAmount,
					details);

				return MindboxLoyaltyOrderResult.Failure(
					MindboxLoyaltyOrderOutcome.BonusesChanged,
					"Сумма списания не совпала с запрошенной",
					operation.HttpStatusCode,
					response.Status,
					response.Order.ProcessingStatus,
					operation.ErrorId,
					operation.DurationMs);
			}

			_logger.LogInformation(
				"CreateAuthorizedLoyaltyOrder: успех. onlineOrderId={OnlineOrderId}, externalOrderId={ExternalOrderId}, transactionId={TransactionId}, spent={Spent}, earned={Earned}, mindboxId={MindboxId}, httpCode={HttpCode}, status={Status}, processingStatus={ProcessingStatus}, errorId={ErrorId}, durationMs={DurationMs}",
				onlineOrderId,
				externalOrderId,
				transactionId,
				change.SpentAmount,
				change.EarnedAmount,
				response.Order.Ids?.MindboxId,
				operation.HttpStatusCode,
				response.Status,
				response.Order.ProcessingStatus,
				operation.ErrorId,
				operation.DurationMs);

			return MindboxLoyaltyOrderResult.Success(
				response.Order.Ids?.MindboxId,
				ToBonusPoints(change.SpentAmount),
				ToBonusPoints(change.EarnedAmount),
				operation.HttpStatusCode,
				response.Status,
				response.Order.ProcessingStatus,
				operation.ErrorId,
				operation.DurationMs);
		}

		private MindboxAuthorizedCartBonusPointsChange GetBonusChange(
			IEnumerable<MindboxAuthorizedCartBonusPointsChange> changes)
		{
			var list = changes?.Where(change => change != null).ToList();

			if(list is null || list.Count == 0)
			{
				return null;
			}

			var balanceSystemName = _settings.BonusBalanceSystemName;

			if(!string.IsNullOrWhiteSpace(balanceSystemName))
			{
				var matching = list.FirstOrDefault(change =>
					change.BalanceType?.Ids != null
					&& change.BalanceType.Ids.TryGetValue("systemName", out var systemName)
					&& string.Equals(systemName, balanceSystemName, StringComparison.Ordinal));

				if(matching != null)
				{
					return matching;
				}
			}

			return list[0];
		}

		private async Task SendAlertAsync(string message, CancellationToken cancellationToken)
		{
			try
			{
				await _alertSender.AlertAsync(message, cancellationToken).ConfigureAwait(false);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Не удалось отправить алерт Mindbox: {Message}", message);
			}
		}

		private static bool IsBonusAmountValidationError(string rawResponse)
		{
			if(string.IsNullOrWhiteSpace(rawResponse))
			{
				return false;
			}

			MindboxResponse response;

			try
			{
				response = JsonSerializer.Deserialize<MindboxResponse>(rawResponse);
			}
			catch(JsonException)
			{
				return false;
			}

			return response?.ValidationMessages?.Any(message =>
			{
				if(message?.Location is null)
				{
					return false;
				}

				var location = message.Location.Replace('.', '/').TrimStart('/');

				return location.IndexOf(BonusPointsLocation, StringComparison.OrdinalIgnoreCase) >= 0;
			}) == true;
		}

		private static int ToBonusPoints(decimal value)
		{
			return (int)Math.Round(value, MidpointRounding.AwayFromZero);
		}
	}
}
