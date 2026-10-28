using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Client.Mindbox;
using Vodovoz.Mindbox.Mapping;
using Vodovoz.Settings.Database.Mindbox;

namespace Vodovoz.Mindbox.Services
{
	/// <inheritdoc />
	public sealed class MindboxCounterpartyOutboxProcessor : IMindboxCounterpartyOutboxProcessor
	{
		private const int MaxErrorLength = 2000;

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IMindboxClient _mindboxClient;
		private readonly IMindboxCustomerRequestFactory _requestFactory;
		private readonly IMindboxRetryPolicy _retryPolicy;
		private readonly IMindboxSendingCircuitBreaker _circuitBreaker;
		private readonly IMindboxAlertSender _alertSender;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxCounterpartyOutboxProcessor> _logger;

		public MindboxCounterpartyOutboxProcessor(
			IUnitOfWorkFactory unitOfWorkFactory,
			IMindboxClient mindboxClient,
			IMindboxCustomerRequestFactory requestFactory,
			IMindboxRetryPolicy retryPolicy,
			IMindboxSendingCircuitBreaker circuitBreaker,
			IMindboxAlertSender alertSender,
			IMindboxSettings settings,
			ILogger<MindboxCounterpartyOutboxProcessor> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_mindboxClient = mindboxClient ?? throw new ArgumentNullException(nameof(mindboxClient));
			_requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
			_retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
			_circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
			_alertSender = alertSender ?? throw new ArgumentNullException(nameof(alertSender));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task<int> ProcessAsync(CancellationToken cancellationToken)
		{
			if(!_settings.IsEnabled)
			{
				return 0;
			}

			var now = DateTime.UtcNow;

			var unavailableAlertMinutes = Math.Max(1, _settings.UnavailableAlertMinutes);

			if(_circuitBreaker.TryMarkUnavailableAlertSent(
				now,
				TimeSpan.FromMinutes(unavailableAlertMinutes)))
			{
				await _alertSender.AlertAsync(
					$"Mindbox недоступен более {unavailableAlertMinutes} минут. Причина: {_circuitBreaker.PauseReason}",
					cancellationToken);
			}

			if(_circuitBreaker.IsPaused(now))
			{
				_logger.LogDebug(
					"Отправка в Mindbox на паузе до {PausedUntil}: {Reason}",
					_circuitBreaker.PausedUntil,
					_circuitBreaker.PauseReason);

				return 0;
			}

			var batchSize = Math.Max(1, _settings.SendBatchSize);
			List<int> messageIds;

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Выборка очереди отправки в Mindbox"))
			{
				messageIds = unitOfWork.Session.Query<MindboxCounterpartyOutboxMessage>()
					.Where(message => message.Status == MindboxCounterpartyOutboxStatus.Queued
						&& (message.NextAttemptAt == null || message.NextAttemptAt <= now))
					.OrderBy(message => message.CreatedAt)
					.Take(batchSize)
					.Select(message => message.Id)
					.ToList();
			}

			foreach(var messageId in messageIds)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					break;
				}

				if(_circuitBreaker.IsPaused(DateTime.UtcNow))
				{
					break;
				}

				try
				{
					using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Отправка контрагента в Mindbox"))
					{
						var message = unitOfWork.GetById<MindboxCounterpartyOutboxMessage>(messageId);

						if(message == null)
						{
							continue;
						}

						await ProcessMessageAsync(unitOfWork, message, cancellationToken);
					}
				}
				catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
				{
					break;
				}
				catch(Exception ex)
				{
					_logger.LogError(
						ex,
						"Не удалось обработать сообщение очереди Mindbox {MessageId}",
						messageId);

					await MarkMessageAsErrorAsync(messageId, ex, cancellationToken);
				}
			}

			return messageIds.Count;
		}

		private async Task MarkMessageAsErrorAsync(
			int messageId,
			Exception exception,
			CancellationToken cancellationToken)
		{
			if(cancellationToken.IsCancellationRequested)
			{
				return;
			}

			try
			{
				using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Ошибка обработки очереди Mindbox"))
				{
					var message = unitOfWork.GetById<MindboxCounterpartyOutboxMessage>(messageId);

					if(message == null)
					{
						return;
					}

					message.Attempts++;
					message.LastAttemptAt = DateTime.UtcNow;
					SetError(message, exception.Message, null);

					unitOfWork.Save(message);
					unitOfWork.Commit();

					await _alertSender.AlertAsync(
						$"Mindbox: неожиданная ошибка обработки контрагента {message.Counterparty?.Id}: {exception.Message}",
						cancellationToken);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Не удалось сохранить ошибку сообщения очереди Mindbox {MessageId}", messageId);
			}
		}

		private async Task ProcessMessageAsync(
			IUnitOfWork unitOfWork,
			MindboxCounterpartyOutboxMessage message,
			CancellationToken cancellationToken)
		{
			var now = DateTime.UtcNow;

			if(message.Attempts == 0)
			{
				// 24 часа отсчитываются от первой попытки, а не от постановки в очередь:
				// при большой исторической выгрузке сообщение может ждать отправки дольше суток.
				message.RetryUntil = now.AddHours(Math.Max(1, _settings.RetryMaxHours));
			}
			else if(message.RetryUntil <= now)
			{
				message.Attempts++;
				message.LastAttemptAt = now;
				SetError(message, "Истек срок повторных попыток (24 часа)", null);
				unitOfWork.Save(message);
				unitOfWork.Commit();

				_logger.LogError(
					"Истек срок повторных попыток Mindbox: erpId={ErpId}, transactionId={TransactionId}, attempts={Attempts}",
					message.Counterparty?.Id,
					message.TransactionId,
					message.Attempts);

				await _alertSender.AlertAsync(
					$"Mindbox: исчерпаны повторы для контрагента {message.Counterparty?.Id}, transactionId {message.TransactionId}",
					cancellationToken);

				return;
			}

			var counterparty = message.Counterparty;

			if(counterparty == null)
			{
				message.Attempts++;
				message.LastAttemptAt = now;
				SetError(message, "Контрагент не найден", null);
				unitOfWork.Save(message);
				unitOfWork.Commit();

				_logger.LogError(
					"Не найден контрагент для сообщения очереди Mindbox {MessageId}",
					message.Id);

				return;
			}

			var request = _requestFactory.Create(unitOfWork, counterparty);

			message.Attempts++;
			message.LastAttemptAt = now;

			var result = await _mindboxClient.CreateCustomerAsync(
				request,
				message.TransactionId,
				cancellationToken);

			await ApplyResultAsync(message, result, now, counterparty.Id, cancellationToken);

			unitOfWork.Save(message);
			unitOfWork.Commit();
		}

		private async Task ApplyResultAsync(
			MindboxCounterpartyOutboxMessage message,
			MindboxOperationResult result,
			DateTime now,
			int counterpartyId,
			CancellationToken cancellationToken)
		{
			message.LastHttpStatusCode = result.HttpStatusCode;
			message.LastMindboxStatus = result.Status;
			message.LastErrorId = result.ErrorId;
			message.LastAttemptDurationMs = result.DurationMs;
			message.LastError = result.IsSuccess ? null : BuildError(result);

			if(result.IsSuccess)
			{
				message.Status = MindboxCounterpartyOutboxStatus.Sent;
				message.SentAt = now;
				message.NextAttemptAt = null;

				_circuitBreaker.Reset();

				_logger.LogInformation(
					"Контрагент отправлен в Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}",
					counterpartyId,
					message.TransactionId,
					_settings.CreateCustomerOperation,
					result.HttpStatusCode,
					result.Status,
					result.ErrorId,
					result.DurationMs,
					message.Attempts);

				return;
			}

			switch(result.Outcome)
			{
				case MindboxSendOutcome.ValidationError:
					message.Status = MindboxCounterpartyOutboxStatus.DataError;
					message.NextAttemptAt = null;

					_logger.LogWarning(
						"Ошибка данных Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}, error={Error}",
						counterpartyId,
						message.TransactionId,
						_settings.CreateCustomerOperation,
						result.HttpStatusCode,
						result.Status,
						result.ErrorId,
						result.DurationMs,
						message.Attempts,
						result.ErrorMessage);

					return;
				case MindboxSendOutcome.ProtocolError:
					message.Status = MindboxCounterpartyOutboxStatus.Error;
					message.NextAttemptAt = null;

					_logger.LogError(
						"Ошибка запроса Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}, error={Error}",
						counterpartyId,
						message.TransactionId,
						_settings.CreateCustomerOperation,
						result.HttpStatusCode,
						result.Status,
						result.ErrorId,
						result.DurationMs,
						message.Attempts,
						result.ErrorMessage);

					await _alertSender.AlertAsync(
						$"Mindbox: ошибка запроса для контрагента {counterpartyId}, HTTP {result.HttpStatusCode}, status {result.Status}, errorId {result.ErrorId}",
						cancellationToken);

					return;
				case MindboxSendOutcome.Unauthorized:
				case MindboxSendOutcome.NotFound:
				case MindboxSendOutcome.RateLimited:
					// Эти ошибки не повторяем: повтор вернёт тот же ответ.
					// Текущий контрагент уходит в «Ошибка», очередь ставится на паузу,
					// чтобы не отправлять остальные запросы в заведомо неисправный Mindbox.
					message.Status = MindboxCounterpartyOutboxStatus.Error;
					message.NextAttemptAt = null;

					var pauseReason = $"HTTP {result.HttpStatusCode} {result.Status}";

					_circuitBreaker.Pause(
						now,
						TimeSpan.FromMinutes(Math.Max(1, _settings.PauseMinutes)),
						pauseReason);

					_logger.LogError(
						"Отправка в Mindbox остановлена: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}, error={Error}",
						counterpartyId,
						message.TransactionId,
						_settings.CreateCustomerOperation,
						result.HttpStatusCode,
						result.Status,
						result.ErrorId,
						result.DurationMs,
						message.Attempts,
						result.ErrorMessage);

					await _alertSender.AlertAsync(
						$"Mindbox: отправка остановлена, HTTP {result.HttpStatusCode}, status {result.Status}. Причина: {result.ErrorMessage}",
						cancellationToken);

					return;
				case MindboxSendOutcome.TransientError:
					var nextAttemptAt = _retryPolicy.GetNextAttemptAt(
						now,
						message.Attempts,
						message.RetryUntil);

					if(nextAttemptAt == null)
					{
						message.Status = MindboxCounterpartyOutboxStatus.Error;
						message.NextAttemptAt = null;

						_logger.LogError(
							"Исчерпаны повторы Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}, error={Error}",
							counterpartyId,
							message.TransactionId,
							_settings.CreateCustomerOperation,
							result.HttpStatusCode,
							result.Status,
							result.ErrorId,
							result.DurationMs,
							message.Attempts,
							result.ErrorMessage);

						await _alertSender.AlertAsync(
							$"Mindbox: исчерпаны повторы для контрагента {counterpartyId}, последняя ошибка: {result.ErrorMessage}",
							cancellationToken);
					}
					else
					{
						message.Status = MindboxCounterpartyOutboxStatus.Queued;
						message.NextAttemptAt = nextAttemptAt;

						_logger.LogWarning(
							"Повтор отправки в Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}, nextAttemptAt={NextAttemptAt}, error={Error}",
							counterpartyId,
							message.TransactionId,
							_settings.CreateCustomerOperation,
							result.HttpStatusCode,
							result.Status,
							result.ErrorId,
							result.DurationMs,
							message.Attempts,
							nextAttemptAt,
							result.ErrorMessage);
					}

					_circuitBreaker.RegisterTransientFailure(
						now,
						$"Временная ошибка Mindbox: HTTP {result.HttpStatusCode} {result.Status}");

					return;
			}
		}

		private static void SetError(
			MindboxCounterpartyOutboxMessage message,
			string error,
			MindboxOperationResult result)
		{
			message.Status = MindboxCounterpartyOutboxStatus.Error;
			message.NextAttemptAt = null;
			message.LastError = Truncate(error, MaxErrorLength);

			if(result != null)
			{
				message.LastHttpStatusCode = result.HttpStatusCode;
				message.LastMindboxStatus = result.Status;
				message.LastErrorId = result.ErrorId;
				message.LastAttemptDurationMs = result.DurationMs;
			}
		}

		private static string BuildError(MindboxOperationResult result)
		{
			var parts = new List<string>();

			if(!string.IsNullOrWhiteSpace(result.Status))
			{
				parts.Add($"status={result.Status}");
			}

			if(!string.IsNullOrWhiteSpace(result.ErrorMessage))
			{
				parts.Add(result.ErrorMessage);
			}

			if(!string.IsNullOrWhiteSpace(result.ErrorId))
			{
				parts.Add($"errorId={result.ErrorId}");
			}

			return Truncate(string.Join("; ", parts), MaxErrorLength);
		}

		private static string Truncate(string value, int maxLength)
		{
			if(string.IsNullOrEmpty(value) || value.Length <= maxLength)
			{
				return value;
			}

			return value.Substring(0, maxLength);
		}
	}
}
