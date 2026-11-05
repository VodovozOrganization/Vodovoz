using MassTransit;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Contracts;
using Mindbox.Library.Mapping;
using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients.Mindbox;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Settings.Database.Mindbox;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Отправляет контрагента в Mindbox по сообщению из outbox шины online_orders.
	/// </summary>
	public sealed class MindboxCounterpartySendConsumer : IConsumer<MindboxCounterpartySendMessage>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IMindboxClient _mindboxClient;
		private readonly IMindboxCustomerRequestFactory _requestFactory;
		private readonly IMindboxSendingCircuitBreaker _circuitBreaker;
		private readonly IMindboxAlertSender _alertSender;
		private readonly IMindboxSettings _settings;
		private readonly ILogger<MindboxCounterpartySendConsumer> _logger;

		public MindboxCounterpartySendConsumer(
			IUnitOfWorkFactory unitOfWorkFactory,
			IMindboxClient mindboxClient,
			IMindboxCustomerRequestFactory requestFactory,
			IMindboxSendingCircuitBreaker circuitBreaker,
			IMindboxAlertSender alertSender,
			IMindboxSettings settings,
			ILogger<MindboxCounterpartySendConsumer> logger)
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
		public async Task Consume(ConsumeContext<MindboxCounterpartySendMessage> context)
		{
			var message = context.Message;
			var now = DateTime.UtcNow;

			if(_circuitBreaker.IsPaused(now))
			{
				throw new MindboxTransientException(
					$"Отправка в Mindbox на паузе до {_circuitBreaker.PausedUntil}: {_circuitBreaker.PauseReason}");
			}

			using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Отправка контрагента в Mindbox"))
			{
				var counterparty = unitOfWork.GetById<CounterpartyEntity>(message.CounterpartyId);

				if(counterparty == null)
				{
					_logger.LogWarning(
						"Контрагент {CounterpartyId} для Mindbox не найден, сообщение пропущено",
						message.CounterpartyId);

					return;
				}

				if(counterparty.MindboxStatus == MindboxCounterpartyOutboxStatus.Sent)
				{
					_logger.LogInformation(
						"Контрагент {CounterpartyId} уже отправлен в Mindbox, повтор пропущен",
						counterparty.Id);

					return;
				}

				var transactionId = ResolveTransactionId(counterparty, message.TransactionId);

				MindboxCustomerRequest request;

				try
				{
					request = _requestFactory.Create(unitOfWork, counterparty);
				}
				catch(Exception ex)
				{
					counterparty.MindboxStatus = MindboxCounterpartyOutboxStatus.DataError;

					unitOfWork.Save(counterparty);
					await unitOfWork.CommitAsync(context.CancellationToken).ConfigureAwait(false);

					_logger.LogError(
						ex,
						"Не удалось подготовить данные контрагента {CounterpartyId} для Mindbox",
						counterparty.Id);

					await _alertSender.AlertAsync(
						$"Mindbox: ошибка данных контрагента {counterparty.Id}: {ex.Message}",
						context.CancellationToken);

					return;
				}

				var result = await _mindboxClient
					.CreateCustomerAsync(request, transactionId, context.CancellationToken)
					.ConfigureAwait(false);

				await ApplyResultAsync(unitOfWork, counterparty, result, transactionId, context, now)
					.ConfigureAwait(false);
			}
		}

		private async Task ApplyResultAsync(
			IUnitOfWork unitOfWork,
			CounterpartyEntity counterparty,
			MindboxOperationResult result,
			Guid transactionId,
			ConsumeContext<MindboxCounterpartySendMessage> context,
			DateTime now)
		{
			if(result.IsSuccess)
			{
				counterparty.MindboxStatus = MindboxCounterpartyOutboxStatus.Sent;

				unitOfWork.Save(counterparty);
				await unitOfWork.CommitAsync(context.CancellationToken).ConfigureAwait(false);

				_circuitBreaker.Reset();

				_logger.LogInformation(
					"Контрагент отправлен в Mindbox: erpId={ErpId}, transactionId={TransactionId}, operation={Operation}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, durationMs={DurationMs}",
					counterparty.Id,
					transactionId,
					_settings.CreateCustomerOperation,
					result.HttpStatusCode,
					result.Status,
					result.ErrorId,
					result.DurationMs);

				return;
			}

			switch(result.Outcome)
			{
				case MindboxSendOutcome.ValidationError:
					await SetTerminalStatusAsync(
						unitOfWork,
						counterparty,
						MindboxCounterpartyOutboxStatus.DataError,
						context,
						"Ошибка данных Mindbox",
						result,
						transactionId);

					return;
				case MindboxSendOutcome.ProtocolError:
					await SetTerminalStatusAsync(
						unitOfWork,
						counterparty,
						MindboxCounterpartyOutboxStatus.Error,
						context,
						"Ошибка запроса Mindbox",
						result,
						transactionId);

					return;
				case MindboxSendOutcome.Unauthorized:
				case MindboxSendOutcome.NotFound:
				case MindboxSendOutcome.RateLimited:
					counterparty.MindboxStatus = MindboxCounterpartyOutboxStatus.Error;

					unitOfWork.Save(counterparty);
					await unitOfWork.CommitAsync(context.CancellationToken).ConfigureAwait(false);

					var pauseReason = $"HTTP {result.HttpStatusCode} {result.Status}";

					_circuitBreaker.Pause(
						now,
						TimeSpan.FromMinutes(Math.Max(1, _settings.PauseMinutes)),
						pauseReason);

					_logger.LogError(
						"Отправка в Mindbox остановлена: erpId={ErpId}, transactionId={TransactionId}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, error={Error}",
						counterparty.Id,
						transactionId,
						result.HttpStatusCode,
						result.Status,
						result.ErrorId,
						result.ErrorMessage);

					await _alertSender.AlertAsync(
						$"Mindbox: отправка остановлена, HTTP {result.HttpStatusCode}, status {result.Status}. Причина: {result.ErrorMessage}",
						context.CancellationToken);

					return;
				case MindboxSendOutcome.TransientError:
					await HandleTransientErrorAsync(unitOfWork, counterparty, result, transactionId, context, now);
					return;
			}
		}

		private async Task HandleTransientErrorAsync(
			IUnitOfWork unitOfWork,
			CounterpartyEntity counterparty,
			MindboxOperationResult result,
			Guid transactionId,
			ConsumeContext<MindboxCounterpartySendMessage> context,
			DateTime now)
		{
			var retryAttempt = context.GetRetryAttempt();
			var maxAttempts = MindboxRetrySchedule.ParseMinutes(_settings.RetryScheduleMinutes).Length;
			var retriesExhausted = retryAttempt >= maxAttempts;

			_circuitBreaker.RegisterTransientFailure(
				now,
				$"Временная ошибка Mindbox: HTTP {result.HttpStatusCode} {result.Status}");

			if(retriesExhausted)
			{
				counterparty.MindboxStatus = MindboxCounterpartyOutboxStatus.Error;

				unitOfWork.Save(counterparty);
				await unitOfWork.CommitAsync(context.CancellationToken).ConfigureAwait(false);

				_logger.LogError(
					"Исчерпаны повторы Mindbox: erpId={ErpId}, transactionId={TransactionId}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, error={Error}",
					counterparty.Id,
					transactionId,
					result.HttpStatusCode,
					result.Status,
					result.ErrorId,
					result.ErrorMessage);

				await _alertSender.AlertAsync(
					$"Mindbox: исчерпаны повторы для контрагента {counterparty.Id}, последняя ошибка: {result.ErrorMessage}",
					context.CancellationToken);

				return;
			}

			counterparty.MindboxStatus = MindboxCounterpartyOutboxStatus.Queued;

			unitOfWork.Save(counterparty);
			await unitOfWork.CommitAsync(context.CancellationToken).ConfigureAwait(false);

			_logger.LogWarning(
				"Повтор отправки в Mindbox: erpId={ErpId}, transactionId={TransactionId}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, attempt={Attempt}, error={Error}",
				counterparty.Id,
				transactionId,
				result.HttpStatusCode,
				result.Status,
				result.ErrorId,
				retryAttempt + 1,
				result.ErrorMessage);

			throw new MindboxTransientException(
				$"Временная ошибка Mindbox для контрагента {counterparty.Id}: HTTP {result.HttpStatusCode} {result.Status}");
		}

		private async Task SetTerminalStatusAsync(
			IUnitOfWork unitOfWork,
			CounterpartyEntity counterparty,
			MindboxCounterpartyOutboxStatus status,
			ConsumeContext<MindboxCounterpartySendMessage> context,
			string logMessage,
			MindboxOperationResult result,
			Guid transactionId)
		{
			counterparty.MindboxStatus = status;

			unitOfWork.Save(counterparty);
			await unitOfWork.CommitAsync(context.CancellationToken).ConfigureAwait(false);

			_logger.LogError(
				"{Message}: erpId={ErpId}, transactionId={TransactionId}, httpCode={HttpCode}, status={Status}, errorId={ErrorId}, error={Error}",
				logMessage,
				counterparty.Id,
				transactionId,
				result.HttpStatusCode,
				result.Status,
				result.ErrorId,
				result.ErrorMessage);

			await _alertSender.AlertAsync(
				$"Mindbox: {logMessage} для контрагента {counterparty.Id}, HTTP {result.HttpStatusCode}, status {result.Status}, errorId {result.ErrorId}",
				context.CancellationToken);
		}

		private Guid ResolveTransactionId(CounterpartyEntity counterparty, Guid messageTransactionId)
		{
			var transactionId = messageTransactionId;

			if(transactionId == Guid.Empty)
			{
				transactionId = counterparty.MindboxTransactionId;
			}

			if(transactionId == Guid.Empty)
			{
				transactionId = Guid.NewGuid();
			}

			if(counterparty.MindboxTransactionId != transactionId)
			{
				counterparty.MindboxTransactionId = transactionId;
			}

			return transactionId;
		}
	}
}
