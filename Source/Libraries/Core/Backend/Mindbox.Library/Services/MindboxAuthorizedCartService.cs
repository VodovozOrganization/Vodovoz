using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxAuthorizedCartService : IMindboxAuthorizedCartService
	{
		private const string SuccessStatus = "Success";
		private const string CalculatedStatus = "Calculated";
		private const string SpentBonusPointsType = "spentBonusPoints";
		private const string DiscountType = "discount";
		private const string BonusPointsLocation = "order/bonusPoints";

		private readonly IMindboxClient _client;
		private readonly IMindboxSettings _settings;
		private readonly IMindboxBonusCircuitBreaker _circuitBreaker;
		private readonly IMindboxAlertSender _alertSender;
		private readonly ILogger<MindboxAuthorizedCartService> _logger;

		public MindboxAuthorizedCartService(
			IMindboxClient client,
			IMindboxSettings settings,
			IMindboxBonusCircuitBreaker circuitBreaker,
			IMindboxAlertSender alertSender,
			ILogger<MindboxAuthorizedCartService> logger)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
			_alertSender = alertSender ?? throw new ArgumentNullException(nameof(alertSender));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task<MindboxAuthorizedCartResult> CalculateAsync(
			MindboxAuthorizedCartRequest request,
			int erpCounterpartyId,
			Guid externalOrderId,
			CancellationToken cancellationToken)
		{
			if(request is null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var requestedAmount = GetRequestedAmount(request);

			if(_circuitBreaker.IsPaused(DateTime.UtcNow))
			{
				_logger.LogWarning(
					"CalculateAuthorizedCart на паузе до {PausedUntil}: erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}",
					_circuitBreaker.PausedUntil,
					erpCounterpartyId,
					externalOrderId);

				return MindboxAuthorizedCartResult.Failure("Mindbox на паузе");
			}

			if(!_circuitBreaker.TryBeginCall(DateTime.UtcNow))
			{
				_logger.LogWarning(
					"CalculateAuthorizedCart пропущен из-за паузы или пробного вызова: erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}",
					erpCounterpartyId,
					externalOrderId);

				return MindboxAuthorizedCartResult.Failure("Mindbox на паузе");
			}

			var operation = await _client.CalculateAuthorizedCartAsync(request, cancellationToken);

			if(operation.Outcome == MindboxSendOutcome.ValidationError
				&& IsBonusAmountValidationError(operation.RawResponse))
			{
				_logger.LogWarning(
					"CalculateAuthorizedCart отклонил сумму списания, повтор с amount=0: erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}, requested={Requested}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}",
					erpCounterpartyId,
					externalOrderId,
					requestedAmount,
					operation.HttpStatusCode,
					operation.Status,
					operation.ErrorId,
					operation.DurationMs);

				if(request.Order?.BonusPoints != null && request.Order.BonusPoints.Count > 0)
				{
					request.Order.BonusPoints[0].Amount = 0;
				}

				operation = await _client.CalculateAuthorizedCartAsync(request, cancellationToken);
			}

			if(!operation.IsSuccess)
			{
				return await HandleFailureAsync(
					operation,
					erpCounterpartyId,
					externalOrderId,
					requestedAmount);
			}

			var result = ParseResponse(
				operation,
				request,
				erpCounterpartyId,
				externalOrderId,
				requestedAmount);

			if(!result.IsSuccess)
			{
				_circuitBreaker.RegisterFailure(DateTime.UtcNow, false, result.ErrorMessage);
			}
			else
			{
				_circuitBreaker.RegisterSuccess();
			}

			_logger.LogInformation(
				"CalculateAuthorizedCart: erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}, requested={Requested}, applied={Applied}, httpCode={HttpCode}, status={Status}, processingStatus={ProcessingStatus}, errorId={ErrorId}, durationMs={DurationMs}, outcome={Outcome}",
				erpCounterpartyId,
				externalOrderId,
				requestedAmount,
				result.BonusesWrittenOff,
				result.HttpStatusCode,
				result.Status,
				result.ProcessingStatus,
				result.ErrorId,
				result.DurationMs,
				result.Outcome);

			return result;
		}

		private MindboxAuthorizedCartResult ParseResponse(
			MindboxOperationResult operation,
			MindboxAuthorizedCartRequest request,
			int erpCounterpartyId,
			Guid externalOrderId,
			int requestedAmount)
		{
			if(string.IsNullOrWhiteSpace(operation.RawResponse))
			{
				return FailParse(
					operation,
					"Mindbox вернул пустой ответ CalculateAuthorizedCart",
					erpCounterpartyId,
					externalOrderId,
					requestedAmount);
			}

			_logger.LogDebug(
				"CalculateAuthorizedCart response parsed: erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}",
				erpCounterpartyId,
				externalOrderId);

			MindboxAuthorizedCartResponse response;

			try
			{
				response = JsonSerializer.Deserialize<MindboxAuthorizedCartResponse>(operation.RawResponse);
			}
			catch(JsonException ex)
			{
				return FailParse(
					operation,
					$"Не удалось разобрать ответ CalculateAuthorizedCart: {ex.Message}",
					erpCounterpartyId,
					externalOrderId,
					requestedAmount);
			}

			if(response?.Order is null)
			{
				return FailParse(
					operation,
					"В ответе CalculateAuthorizedCart нет order",
					erpCounterpartyId,
					externalOrderId,
					requestedAmount);
			}

			if(!string.Equals(response.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase))
			{
				return FailParse(
					operation,
					$"Статус CalculateAuthorizedCart: {response.Status}",
					erpCounterpartyId,
					externalOrderId,
					requestedAmount,
					response.Status,
					response.Order.ProcessingStatus);
			}

			if(!string.Equals(response.Order.ProcessingStatus, CalculatedStatus, StringComparison.OrdinalIgnoreCase))
			{
				_logger.LogWarning(
					"CalculateAuthorizedCart: processingStatus={ProcessingStatus}, erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}",
					response.Order.ProcessingStatus,
					erpCounterpartyId,
					externalOrderId);

				return MindboxAuthorizedCartResult.Failure(
					$"Статус обработки заказа CalculateAuthorizedCart: {response.Order.ProcessingStatus}",
					operation.HttpStatusCode,
					response.Status,
					response.Order.ProcessingStatus,
					operation.ErrorId,
					operation.Outcome,
					operation.DurationMs);
			}

			var bonusInfo = response.Order.TotalBonusPointsInfo
				?? ResolveBonusInfo(response.Order.BonusPointsInfoPerBalanceType);

			if(bonusInfo is null)
			{
				return FailParse(
					operation,
					"В ответе CalculateAuthorizedCart нет totalBonusPointsInfo",
					erpCounterpartyId,
					externalOrderId,
					requestedAmount,
					response.Status,
					response.Order.ProcessingStatus);
			}

			if(!string.IsNullOrWhiteSpace(bonusInfo.Status)
				&& !string.Equals(bonusInfo.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase))
			{
				return FailParse(
					operation,
					$"Статус бонусов CalculateAuthorizedCart: {bonusInfo.Status}",
					erpCounterpartyId,
					externalOrderId,
					requestedAmount,
					response.Status,
					response.Order.ProcessingStatus);
			}

			if(response.Order.Lines is null)
			{
				return FailParse(
					operation,
					"В ответе CalculateAuthorizedCart нет lines",
					erpCounterpartyId,
					externalOrderId,
					requestedAmount,
					response.Status,
					response.Order.ProcessingStatus);
			}

			var linesByNumber = response.Order.Lines
				.Where(line => line != null)
				.GroupBy(line => line.LineNumber)
				.ToDictionary(group => group.Key, group => group.First());

			var bonusesByLineNumber = new Dictionary<int, int>();

			foreach(var requestLine in request.Order?.Lines ?? new List<MindboxAuthorizedCartLine>())
			{
				if(!linesByNumber.TryGetValue(requestLine.LineNumber, out var responseLine))
				{
					_logger.LogError(
						"CalculateAuthorizedCart: в ответе нет строки lineNumber={LineNumber}, erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}",
						requestLine.LineNumber,
						erpCounterpartyId,
						externalOrderId);

					return FailParse(
						operation,
						$"В ответе нет строки {requestLine.LineNumber}",
						erpCounterpartyId,
						externalOrderId,
						requestedAmount,
						response.Status,
						response.Order.ProcessingStatus);
				}

				var spentBonusPoints = 0;

				foreach(var promotion in responseLine.AppliedPromotions ?? new List<MindboxAppliedPromotion>())
				{
					if(promotion is null)
					{
						continue;
					}

					if(string.Equals(promotion.Type, SpentBonusPointsType, StringComparison.OrdinalIgnoreCase))
					{
						spentBonusPoints += ConvertToBonusPoints(promotion.Amount);
					}
					else if(string.Equals(promotion.Type, DiscountType, StringComparison.OrdinalIgnoreCase))
					{
						_logger.LogWarning(
							"CalculateAuthorizedCart: скидочная акция в позиции проигнорирована, lineNumber={LineNumber}, erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}",
							responseLine.LineNumber,
							erpCounterpartyId,
							externalOrderId);
					}
				}

				bonusesByLineNumber[responseLine.LineNumber] = spentBonusPoints;
			}

			var linesSum = bonusesByLineNumber.Values.Sum();
			var spentAmount = ConvertToBonusPoints(bonusInfo.SpentAmountForCurrentOrder);

			if(linesSum != spentAmount)
			{
				_logger.LogError(
					"CalculateAuthorizedCart: сумма списания по позициям {LinesSum} не равна spentAmountForCurrentOrder {SpentAmount}, erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}",
					linesSum,
					spentAmount,
					erpCounterpartyId,
					externalOrderId);

				return FailParse(
					operation,
					"Сумма списания по позициям не равна spentAmountForCurrentOrder",
					erpCounterpartyId,
					externalOrderId,
					requestedAmount,
					response.Status,
					response.Order.ProcessingStatus);
			}

			var bonusesToEarn = GetEarnedAmount(response.Order.BonusPointsChanges);

			return MindboxAuthorizedCartResult.Success(
				bonusInfo.Balance is null
					? (int?)null
					: ConvertToBonusPoints(bonusInfo.Balance.Available),
				ConvertToBonusPoints(bonusInfo.AvailableAmountForCurrentOrder),
				spentAmount,
				bonusesToEarn,
				bonusesByLineNumber,
				operation.HttpStatusCode,
				response.Status,
				response.Order.ProcessingStatus,
				operation.ErrorId,
				operation.DurationMs);
		}

		private MindboxAuthorizedCartResult FailParse(
			MindboxOperationResult operation,
			string message,
			int erpCounterpartyId,
			Guid externalOrderId,
			int requestedAmount,
			string status = null,
			string processingStatus = null)
		{
			_logger.LogError(
				"CalculateAuthorizedCart: {Message}, erpCounterpartyId={ErpCounterpartyId}, externalOrderId={ExternalOrderId}, requested={Requested}, httpCode={HttpCode}, status={Status}, processingStatus={ProcessingStatus}, errorId={ErrorId}, durationMs={DurationMs}",
				message,
				erpCounterpartyId,
				externalOrderId,
				requestedAmount,
				operation.HttpStatusCode,
				status ?? operation.Status,
				processingStatus,
				operation.ErrorId,
				operation.DurationMs);

			return MindboxAuthorizedCartResult.Failure(
				message,
				operation.HttpStatusCode,
				status ?? operation.Status,
				processingStatus,
				operation.ErrorId,
				operation.Outcome,
				operation.DurationMs);
		}

		private async Task<MindboxAuthorizedCartResult> HandleFailureAsync(
			MindboxOperationResult operation,
			int erpCounterpartyId,
			Guid externalOrderId,
			int requestedAmount)
		{
			var details = string.Format(
				CultureInfo.InvariantCulture,
				"erpCounterpartyId={0}, externalOrderId={1}, requested={2}, httpCode={3}, status={4}, errorId={5}, durationMs={6}, outcome={7}",
				erpCounterpartyId,
				externalOrderId,
				requestedAmount,
				operation.HttpStatusCode,
				operation.Status,
				operation.ErrorId,
				operation.DurationMs,
				operation.Outcome);

			switch(operation.Outcome)
			{
				case MindboxSendOutcome.ValidationError:
					_logger.LogError("CalculateAuthorizedCart: ошибка в данных ERP. {Details}", details);
					await SendAlertAsync("Mindbox CalculateAuthorizedCart: ошибка данных запроса", cancellationToken: CancellationToken.None);
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, false, null);
					break;
				case MindboxSendOutcome.ProtocolError:
					_logger.LogError("CalculateAuthorizedCart: ошибка запроса. {Details}", details);
					await SendAlertAsync("Mindbox CalculateAuthorizedCart: ошибка запроса (400)", CancellationToken.None);
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, false, null);
					break;
				case MindboxSendOutcome.NotFound:
					_logger.LogError("CalculateAuthorizedCart: операция не найдена. {Details}", details);
					await SendAlertAsync("Mindbox CalculateAuthorizedCart: операция не найдена (404)", CancellationToken.None);
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, false, null);
					break;
				case MindboxSendOutcome.Unauthorized:
					_logger.LogError("CalculateAuthorizedCart: неверный секретный ключ или нет доступа. {Details}", details);
					await SendAlertAsync("Mindbox CalculateAuthorizedCart: неверный секретный ключ (401/403)", CancellationToken.None);
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, false, null);
					break;
				case MindboxSendOutcome.RateLimited:
					_logger.LogError("CalculateAuthorizedCart: превышен лимит вызовов. {Details}", details);
					await SendAlertAsync("Mindbox CalculateAuthorizedCart: превышен лимит вызовов (429)", CancellationToken.None);
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, true, "RateLimited");
					break;
				case MindboxSendOutcome.TransientError:
					_logger.LogWarning("CalculateAuthorizedCart: временная ошибка. {Details}", details);
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, true, operation.Status ?? "TransientError");
					break;
				default:
					_logger.LogError("CalculateAuthorizedCart: неизвестная ошибка. {Details}", details);
					_circuitBreaker.RegisterFailure(DateTime.UtcNow, false, null);
					break;
			}

			return MindboxAuthorizedCartResult.Failure(
				operation.ErrorMessage,
				operation.HttpStatusCode,
				operation.Status,
				null,
				operation.ErrorId,
				operation.Outcome,
				operation.DurationMs);
		}

		private async Task SendAlertAsync(string message, CancellationToken cancellationToken)
		{
			try
			{
				await _alertSender.AlertAsync(message, cancellationToken);
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

			if(response?.ValidationMessages is null)
			{
				return false;
			}

			return response.ValidationMessages.Any(message =>
			{
				if(message?.Location is null)
				{
					return false;
				}

				var normalizedLocation = message.Location
					.Replace('.', '/')
					.TrimStart('/');

				return normalizedLocation.IndexOf(BonusPointsLocation, StringComparison.OrdinalIgnoreCase) >= 0;
			});
		}

		private static int GetRequestedAmount(MindboxAuthorizedCartRequest request)
		{
			return request.Order?.BonusPoints != null && request.Order.BonusPoints.Count > 0
				? request.Order.BonusPoints[0].Amount
				: 0;
		}

		private MindboxAuthorizedCartTotalBonusPointsInfo ResolveBonusInfo(
			IEnumerable<MindboxAuthorizedCartTotalBonusPointsInfo> bonusInfos)
		{
			var infos = bonusInfos?.Where(info => info != null).ToList();

			if(infos is null || infos.Count == 0)
			{
				return null;
			}

			var balanceSystemName = _settings.BonusBalanceSystemName;

			if(!string.IsNullOrWhiteSpace(balanceSystemName))
			{
				var matchingInfo = infos.FirstOrDefault(info =>
					info.BalanceType?.Ids != null
					&& info.BalanceType.Ids.TryGetValue("systemName", out var systemName)
					&& string.Equals(systemName, balanceSystemName, StringComparison.Ordinal));

				if(matchingInfo != null)
				{
					return matchingInfo;
				}
			}

			return infos[0];
		}

		private int GetEarnedAmount(IEnumerable<MindboxAuthorizedCartBonusPointsChange> changes)
		{
			if(changes is null)
			{
				return 0;
			}

			var changesList = changes.Where(change => change != null).ToList();

			if(changesList.Count == 0)
			{
				return 0;
			}

			var balanceSystemName = _settings.BonusBalanceSystemName;

			if(string.IsNullOrWhiteSpace(balanceSystemName))
			{
				return ConvertToBonusPoints(changesList[0].EarnedAmount);
			}

			foreach(var change in changesList)
			{
				if(change.BalanceType?.Ids != null
					&& change.BalanceType.Ids.TryGetValue("systemName", out var systemName)
					&& string.Equals(systemName, balanceSystemName, StringComparison.Ordinal))
				{
					return ConvertToBonusPoints(change.EarnedAmount);
				}
			}

			return 0;
		}

		private static int ConvertToBonusPoints(decimal amount)
		{
			return (int)Math.Round(amount, MidpointRounding.AwayFromZero);
		}
	}
}
