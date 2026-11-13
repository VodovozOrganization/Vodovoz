using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using Mindbox.Library.Mapping;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Mindbox;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <inheritdoc />
	public sealed class MindboxCounterpartySendService : IMindboxCounterpartySendService
	{
		private const int MaxErrorLength = 2000;

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IMindboxClient _mindboxClient;
		private readonly IMindboxCustomerRequestFactory _requestFactory;
		private readonly IMindboxSendingCircuitBreaker _circuitBreaker;
		private readonly IMindboxAlertSender _alertSender;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxCounterpartySendService> _logger;

		public MindboxCounterpartySendService(
			IUnitOfWorkFactory unitOfWorkFactory,
			IMindboxClient mindboxClient,
			IMindboxCustomerRequestFactory requestFactory,
			IMindboxSendingCircuitBreaker circuitBreaker,
			IMindboxAlertSender alertSender,
			IMindboxSettings settings,
			ILogger<MindboxCounterpartySendService> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_mindboxClient = mindboxClient ?? throw new ArgumentNullException(nameof(mindboxClient));
			_requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
			_circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
			_alertSender = alertSender ?? throw new ArgumentNullException(nameof(alertSender));
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public async Task SendAsync(
			int counterpartyId,
			Guid transactionId,
			int retryAttempt,
			CancellationToken cancellationToken)
		{
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
				throw new MindboxTransientException(
					$"Отправка в Mindbox на паузе до {_circuitBreaker.PausedUntil}: {_circuitBreaker.PauseReason}");
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Отправка контрагента в Mindbox"))
			{
				var counterparty = unitOfWork.GetById<CounterpartyEntity>(counterpartyId);

				if(counterparty is null)
				{
					_logger.LogWarning(
						"Контрагент {CounterpartyId} для Mindbox не найден, сообщение пропущено",
						counterpartyId);

					return;
				}

				var state = unitOfWork.GetById<MindboxCounterpartyState>(counterparty.Id);
				var isNewState = state is null;

				if(isNewState)
				{
					state = new MindboxCounterpartyState
					{
						CounterpartyId = counterparty.Id,
						CreatedAt = now,
						UpdatedAt = now
					};
				}

				if(state.Status is MindboxCounterpartyOutboxStatus.Sent)
				{
					_logger.LogInformation(
						"Контрагент {CounterpartyId} уже отправлен в Mindbox, повтор пропущен",
						counterparty.Id);

					return;
				}

				if(!state.CreatedAt.HasValue)
				{
					state.CreatedAt = now;
				}

				var resolvedTransactionId = ResolveTransactionId(state, transactionId);

				MindboxCustomerRequest request;

				try
				{
					request = _requestFactory.Create(unitOfWork, counterparty);
				}
				catch(Exception ex)
				{
					state.Status = MindboxCounterpartyOutboxStatus.DataError;
					state.Attempts++;
					state.LastAttemptAt = now;
					state.LastError = Truncate(ex.Message, MaxErrorLength);
					state.UpdatedAt = now;

					await SaveStateAsync(unitOfWork, state, isNewState, cancellationToken).ConfigureAwait(false);

					_logger.LogError(
						ex,
						"Не удалось подготовить данные контрагента {CounterpartyId} для Mindbox",
						counterparty.Id);

					await _alertSender.AlertAsync(
						$"Mindbox: ошибка данных контрагента {counterparty.Id}: {ex.Message}",
						cancellationToken);

					return;
				}

				var result = await _mindboxClient
					.CreateCustomerAsync(request, resolvedTransactionId, cancellationToken)
					.ConfigureAwait(false);

				await ApplyResultAsync(
					unitOfWork,
					state,
					isNewState,
					counterparty.Id,
					result,
					resolvedTransactionId,
					retryAttempt,
					cancellationToken,
					now)
					.ConfigureAwait(false);
			}
		}

		private async Task ApplyResultAsync(
			IUnitOfWork unitOfWork,
			MindboxCounterpartyState state,
			bool isNewState,
			int counterpartyId,
			MindboxOperationResult result,
			Guid transactionId,
			int retryAttempt,
			CancellationToken cancellationToken,
			DateTime now)
		{
			state.Attempts++;
			state.LastAttemptAt = now;
			state.LastHttpStatusCode = result.HttpStatusCode;
			state.LastMindboxStatus = result.Status;
			state.LastErrorId = result.ErrorId;
			state.LastAttemptDurationMs = result.DurationMs > int.MaxValue
				? int.MaxValue
				: (int)result.DurationMs;
			state.LastError = result.IsSuccess ? null : BuildError(result);
			state.UpdatedAt = now;

			if(result.IsSuccess)
			{
				state.Status = MindboxCounterpartyOutboxStatus.Sent;
				state.SentAt = now;

				await SaveStateAsync(unitOfWork, state, isNewState, cancellationToken).ConfigureAwait(false);

				_circuitBreaker.Reset();

				_logger.LogInformation(
					"Контрагент отправлен в Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}",
					counterpartyId,
					transactionId,
					_settings.CreateCustomerOperation,
					result.HttpStatusCode,
					result.Status,
					result.ErrorId,
					result.DurationMs,
					state.Attempts);

				return;
			}

			switch(result.Outcome)
			{
				case MindboxSendOutcome.ValidationError:
					state.Status = MindboxCounterpartyOutboxStatus.DataError;

					await SaveStateAsync(unitOfWork, state, isNewState, cancellationToken).ConfigureAwait(false);

					_logger.LogWarning(
						"Ошибка данных Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}, error={Error}",
						counterpartyId,
						transactionId,
						_settings.CreateCustomerOperation,
						result.HttpStatusCode,
						result.Status,
						result.ErrorId,
						result.DurationMs,
						state.Attempts,
						result.ErrorMessage);

					return;
				case MindboxSendOutcome.ProtocolError:
					state.Status = MindboxCounterpartyOutboxStatus.Error;

					await SaveStateAsync(unitOfWork, state, isNewState, cancellationToken).ConfigureAwait(false);

					_logger.LogError(
						"Ошибка запроса Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}, error={Error}",
						counterpartyId,
						transactionId,
						_settings.CreateCustomerOperation,
						result.HttpStatusCode,
						result.Status,
						result.ErrorId,
						result.DurationMs,
						state.Attempts,
						result.ErrorMessage);

					await _alertSender.AlertAsync(
						$"Mindbox: ошибка запроса для контрагента {counterpartyId}, HTTP {result.HttpStatusCode}, status {result.Status}, errorId {result.ErrorId}",
						cancellationToken);

					return;
				case MindboxSendOutcome.Unauthorized:
				case MindboxSendOutcome.NotFound:
				case MindboxSendOutcome.RateLimited:
					state.Status = MindboxCounterpartyOutboxStatus.Error;

					await SaveStateAsync(unitOfWork, state, isNewState, cancellationToken).ConfigureAwait(false);

					var pauseReason = $"HTTP {result.HttpStatusCode} {result.Status}";

					_circuitBreaker.Pause(
						now,
						TimeSpan.FromMinutes(Math.Max(1, _settings.PauseMinutes)),
						pauseReason);

					_logger.LogError(
						"Отправка в Mindbox остановлена: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}, error={Error}",
						counterpartyId,
						transactionId,
						_settings.CreateCustomerOperation,
						result.HttpStatusCode,
						result.Status,
						result.ErrorId,
						result.DurationMs,
						state.Attempts,
						result.ErrorMessage);

					await _alertSender.AlertAsync(
						$"Mindbox: отправка остановлена, HTTP {result.HttpStatusCode}, status {result.Status}. Причина: {result.ErrorMessage}",
						cancellationToken);

					return;
				case MindboxSendOutcome.TransientError:
					await HandleTransientErrorAsync(
						unitOfWork,
						state,
						isNewState,
						counterpartyId,
						result,
						transactionId,
						retryAttempt,
						cancellationToken,
						now)
						.ConfigureAwait(false);

					return;
			}
		}

		private async Task HandleTransientErrorAsync(
			IUnitOfWork unitOfWork,
			MindboxCounterpartyState state,
			bool isNewState,
			int counterpartyId,
			MindboxOperationResult result,
			Guid transactionId,
			int retryAttempt,
			CancellationToken cancellationToken,
			DateTime now)
		{
			var maxAttempts = MindboxRetrySchedule.GetMaxAttempts(
				_settings.RetryScheduleMinutes,
				_settings.RetryIntervalMinutes,
				_settings.RetryMaxHours);
			var retriesExhausted = retryAttempt >= maxAttempts;

			_circuitBreaker.RegisterTransientFailure(
				now,
				$"Временная ошибка Mindbox: HTTP {result.HttpStatusCode} {result.Status}");

			if(retriesExhausted)
			{
				state.Status = MindboxCounterpartyOutboxStatus.Error;

				await SaveStateAsync(unitOfWork, state, isNewState, cancellationToken).ConfigureAwait(false);

				_logger.LogError(
					"Исчерпаны повторы Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}, error={Error}",
					counterpartyId,
					transactionId,
					_settings.CreateCustomerOperation,
					result.HttpStatusCode,
					result.Status,
					result.ErrorId,
					result.DurationMs,
					state.Attempts,
					result.ErrorMessage);

				await _alertSender.AlertAsync(
					$"Mindbox: исчерпаны повторы для контрагента {counterpartyId}, последняя ошибка: {result.ErrorMessage}",
					cancellationToken);

				return;
			}

			state.Status = MindboxCounterpartyOutboxStatus.Queued;

			await SaveStateAsync(unitOfWork, state, isNewState, cancellationToken).ConfigureAwait(false);

			_logger.LogWarning(
				"Повтор отправки в Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}, attempt={Attempt}, error={Error}",
				counterpartyId,
				transactionId,
				_settings.CreateCustomerOperation,
				result.HttpStatusCode,
				result.Status,
				result.ErrorId,
				result.DurationMs,
				state.Attempts,
				result.ErrorMessage);

			throw new MindboxTransientException(
				$"Временная ошибка Mindbox для контрагента {counterpartyId}: HTTP {result.HttpStatusCode} {result.Status}");
		}

		private static async Task SaveStateAsync(
			IUnitOfWork unitOfWork,
			MindboxCounterpartyState state,
			bool isNewState,
			CancellationToken cancellationToken)
		{
			if(isNewState)
			{
				// Id задаётся вручную (counterparty_id), поэтому SaveOrUpdate не подходит:
				// для новой записи нужен явный Insert.
				unitOfWork.Session.Save(state);
			}
			else
			{
				unitOfWork.Save(state);
			}

			await unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
		}

		private static Guid ResolveTransactionId(MindboxCounterpartyState state, Guid messageTransactionId)
		{
			var transactionId = messageTransactionId;

			if(transactionId == Guid.Empty)
			{
				transactionId = state.TransactionId ?? Guid.Empty;
			}

			if(transactionId == Guid.Empty)
			{
				transactionId = Guid.NewGuid();
			}

			if(state.TransactionId != transactionId)
			{
				state.TransactionId = transactionId;
			}

			return transactionId;
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
