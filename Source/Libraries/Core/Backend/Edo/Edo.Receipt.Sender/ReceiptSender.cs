using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Validation;
using Microsoft.Extensions.Logging;
using ModulKassa;
using ModulKassa.DTO;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Admin;
using Edo.Common;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Settings.Edo;

namespace Edo.Receipt.Sender
{
	public class ReceiptSender : IDisposable
	{
		private readonly ILogger<ReceiptSender> _logger;
		private readonly IUnitOfWork _uow;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;
		private readonly CashboxClientProvider _cashboxClientProvider;
		private readonly FiscalDocumentFactory _fiscalDocumentFactory;
		private readonly EdoTaskValidator _edoTaskValidator;
		private readonly EdoCancellationService _edoCancellationService;
		private readonly IEdoReceiptSettings _edoReceiptSettings;
		private readonly ReceiptSendingFailedNotificationService _receiptSendingFailedNotificationService;

		public ReceiptSender(
			ILogger<ReceiptSender> logger,
			IUnitOfWork uow,
			EdoProblemRegistrar edoProblemRegistrar,
			CashboxClientProvider cashboxClientProvider,
			FiscalDocumentFactory fiscalDocumentFactory,
			EdoTaskValidator edoTaskValidator,
			EdoCancellationService edoCancellationService,
			IEdoReceiptSettings edoReceiptSettings,
			ReceiptSendingFailedNotificationService receiptSendingFailedNotificationService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
			_cashboxClientProvider = cashboxClientProvider ?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_fiscalDocumentFactory = fiscalDocumentFactory ?? throw new ArgumentNullException(nameof(fiscalDocumentFactory));
			_edoTaskValidator = edoTaskValidator ?? throw new ArgumentNullException(nameof(edoTaskValidator));
			_edoCancellationService = edoCancellationService ?? throw new ArgumentNullException(nameof(edoCancellationService));
			_edoReceiptSettings = edoReceiptSettings ?? throw new ArgumentNullException(nameof(edoReceiptSettings));
			_receiptSendingFailedNotificationService = receiptSendingFailedNotificationService
				?? throw new ArgumentNullException(nameof(receiptSendingFailedNotificationService));
		}

		public async Task HandleReceiptSendEvent(int edoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = _uow.GetById<ReceiptEdoTask>(edoTaskId);
			if(edoTask == null)
			{
				_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} не найдена.",
					edoTaskId);
				return;
			}

			if(edoTask.ReceiptStatus != EdoReceiptStatus.Sending)
			{
				_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
					"находится в статусе {receiptTaskStatus} , " +
					"а должна быть в статусе {sendingStatus}.",
					edoTaskId, edoTask.ReceiptStatus, EdoReceiptStatus.Sending);
				return;
			}

			if(_edoCancellationService.IsEdoTaskMustBeCancelled(edoTask))
			{
				var reason = "Проблема с составом заказа. Сумма заказа или одна из позиций заказа меньше нуля";
				
				await _edoCancellationService.CancelTask(edoTaskId, reason, false, cancellationToken);
				return;
			}
			
			var isValid = await _edoTaskValidator.Validate(edoTask, cancellationToken);
			if(!isValid)
			{
				return;
			}

			switch(edoTask.Status)
			{
				case EdoTaskStatus.New:
					_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
						"новая и не находится в работе.",
						edoTaskId);
					return;
				case EdoTaskStatus.Waiting:
					_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
						"находится в ожидании решения внешних факторов и не находится в работе.",
						edoTaskId);
					return;
				case EdoTaskStatus.Problem:
					_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
						"имеет не решенную проблему.",
						edoTaskId);
					return;
				case EdoTaskStatus.Completed:
					_logger.LogWarning("Невозможно отправить чек. Задача №{edoTaskId} " +
						"уже завершена.",
						edoTaskId);
					return;
				case EdoTaskStatus.InProgress:
					// Корректный статус задачи, продолжаем выполнение
					break;
				default:
					throw new InvalidOperationException($"Неизвестный статус задачи ЭДО {edoTask.Status}");
			}

			if(edoTask.CashboxId == null)
			{
				throw new InvalidOperationException("Не указана касса для отправки чека. Должна проверяться валидацией задачи");
			}

			if(ReceiptSendPauseTimeHelper.IsNightPauseTime(
				DateTime.Now.TimeOfDay,
				_edoReceiptSettings.ReceiptSendPauseStartTime,
				_edoReceiptSettings.ReceiptSendPauseEndTime))
			{
				_logger.LogInformation(
					"Отправка чека по задаче №{edoTaskId} отложена до утра. Текущее время попадает в ночное окно {Start}-{End}.",
					edoTask.Id,
					_edoReceiptSettings.ReceiptSendPauseStartTime,
					_edoReceiptSettings.ReceiptSendPauseEndTime);

				await _edoProblemRegistrar.RegisterCustomProblem<ReceiptSendPausedByNightTime>(
					edoTask,
					cancellationToken);

				return;
			}

			try
			{
				var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(edoTask.CashboxId.Value, cancellationToken);
					
				// отправка чеков
				foreach(var edoFiscalDocument in edoTask.FiscalDocuments)
				{
					var fiscalDocument =  _fiscalDocumentFactory.CreateFiscalDocument(edoFiscalDocument);
					var result = await cashboxClient.CheckFiscalDocument(fiscalDocument, cancellationToken);
					if(result.SendStatus == SendStatus.Error)
					{
						result = await cashboxClient.SendFiscalDocument(fiscalDocument, cancellationToken);
					}
					if(result.SendStatus == SendStatus.Error)
					{
						edoFiscalDocument.FailureMessage = result.ErrorMessage;
						edoFiscalDocument.Status = Vodovoz.Core.Domain.Edo.FiscalDocumentStatus.SendError;
						continue;
					}

					_logger.LogInformation("Чек №{documentNumber} отправлен успешно.", edoFiscalDocument.DocumentNumber);

					edoFiscalDocument.Stage = FiscalDocumentStage.Sent;
					edoFiscalDocument.Status = ReceiptConverters.ConvertFiscalDocumentStatus(result.FiscalDocumentInfo.Status);
					edoFiscalDocument.StatusChangeTime = DateTime.Parse(result.FiscalDocumentInfo.TimeStatusChangedString);
					if(result.FiscalDocumentInfo.FiscalInfo != null)
					{
						edoFiscalDocument.FiscalTime = DateTime.Parse(result.FiscalDocumentInfo.FiscalInfo.Date);
						edoFiscalDocument.FiscalNumber = result.FiscalDocumentInfo.FiscalInfo.FnDocNumber.ToString();
						edoFiscalDocument.FiscalMark = result.FiscalDocumentInfo.FiscalInfo.FnDocMark.ToString();
						edoFiscalDocument.FiscalKktNumber = result.FiscalDocumentInfo.FiscalInfo.KktNumber;
					}
				}

				var hasFailure = edoTask.FiscalDocuments.Any(x => x.Status == Vodovoz.Core.Domain.Edo.FiscalDocumentStatus.Failed);
				var hasSendErrors = edoTask.FiscalDocuments.Any(x => x.Status == Vodovoz.Core.Domain.Edo.FiscalDocumentStatus.SendError);
				if(hasFailure || hasSendErrors)
				{
					var problemDocuments = edoTask.FiscalDocuments
						.Where(x => x.Status == Vodovoz.Core.Domain.Edo.FiscalDocumentStatus.Failed
							|| x.Status == Vodovoz.Core.Domain.Edo.FiscalDocumentStatus.SendError)
						.ToList();

					var problemSourceTypes = problemDocuments
						.Select(x => ReceiptSendFailureClassifier.Classify(x.FailureMessage))
						.Distinct()
						.ToList();

					var details = BuildReceiptSendingFailedDetails(problemDocuments);
					var orderId = edoTask.FormalEdoRequest?.Order?.Id;
					var cashboxId = edoTask.CashboxId;
					var problemSourceNames = problemSourceTypes
						.Select(ReceiptSendFailureClassifier.GetSourceName)
						.ToList();

					_logger.LogWarning(
						"Не удалось отправить некоторые чеки по задаче №{edoTaskId}. Источники: {ProblemSources}. {Details}",
						edoTask.Id,
						string.Join(", ", problemSourceNames),
						details);

					if(hasSendErrors)
					{
						await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
						await _uow.CommitAsync(cancellationToken);
					}

					await RegisterReceiptSendProblems(edoTask, problemSourceTypes, cancellationToken);

					try
					{
						await _receiptSendingFailedNotificationService.NotifyAsync(
							edoTask.Id,
							orderId,
							cashboxId,
							problemSourceNames,
							details,
							cancellationToken);
					}
					catch(Exception notificationException)
					{
						_logger.LogError(
							notificationException,
							"Не удалось отправить уведомление об ошибке отправки чека по задаче №{edoTaskId}",
							edoTask.Id);
					}

					return;
				}

				SolveReceiptSendProblems(edoTask);

				edoTask.ReceiptStatus = EdoReceiptStatus.Sent;
			}
			catch(CashboxException ex)
			{
				_logger.LogWarning(ex, "Ошибка при отправке чека по задаче №{edoTaskId}", edoTask.Id);
				throw;
			}

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			_logger.LogInformation("Все чеки по задаче №{edoTaskId} отправлены успешно.", edoTask.Id);
		}

		private async Task RegisterReceiptSendProblems(
			ReceiptEdoTask edoTask,
			IReadOnlyCollection<Type> problemSourceTypes,
			CancellationToken cancellationToken)
		{
			var sources = problemSourceTypes?.Count > 0
				? problemSourceTypes.ToList()
				: new List<Type> { typeof(ReceiptSendingFailed) };

			for(var i = 0; i < sources.Count; i++)
			{
				var disposeTaskUow = i == sources.Count - 1;
				await RegisterReceiptSendProblem(edoTask, sources[i], disposeTaskUow, cancellationToken);
			}
		}

		private async Task RegisterReceiptSendProblem(
			ReceiptEdoTask edoTask,
			Type problemSourceType,
			bool disposeTaskUow,
			CancellationToken cancellationToken)
		{
			if(problemSourceType == typeof(ReceiptSendHttpBadRequest))
			{
				await _edoProblemRegistrar.RegisterCustomProblem<ReceiptSendHttpBadRequest>(
					edoTask, cancellationToken, disposeTaskUow: disposeTaskUow);
				return;
			}

			if(problemSourceType == typeof(ReceiptSendDocumentStatusNotFound))
			{
				await _edoProblemRegistrar.RegisterCustomProblem<ReceiptSendDocumentStatusNotFound>(
					edoTask, cancellationToken, disposeTaskUow: disposeTaskUow);
				return;
			}

			if(problemSourceType == typeof(ReceiptSendSslError))
			{
				await _edoProblemRegistrar.RegisterCustomProblem<ReceiptSendSslError>(
					edoTask, cancellationToken, disposeTaskUow: disposeTaskUow);
				return;
			}

			if(problemSourceType == typeof(ReceiptSendTransportError))
			{
				await _edoProblemRegistrar.RegisterCustomProblem<ReceiptSendTransportError>(
					edoTask, cancellationToken, disposeTaskUow: disposeTaskUow);
				return;
			}

			await _edoProblemRegistrar.RegisterCustomProblem<ReceiptSendingFailed>(
				edoTask, cancellationToken, disposeTaskUow: disposeTaskUow);
		}

		private void SolveReceiptSendProblems(ReceiptEdoTask edoTask)
		{
			_edoProblemRegistrar.SolveCustomProblem<ReceiptSendHttpBadRequest>(edoTask);
			_edoProblemRegistrar.SolveCustomProblem<ReceiptSendDocumentStatusNotFound>(edoTask);
			_edoProblemRegistrar.SolveCustomProblem<ReceiptSendSslError>(edoTask);
			_edoProblemRegistrar.SolveCustomProblem<ReceiptSendTransportError>(edoTask);
			_edoProblemRegistrar.SolveCustomProblem<ReceiptSendingFailed>(edoTask);
			// Историческое имя проблемы до переименования
			_edoProblemRegistrar.SolveCustomProblem(edoTask, "Custom.NotAllReceiptsWasSended");
		}

		private static string BuildReceiptSendingFailedDetails(
			IReadOnlyCollection<EdoFiscalDocument> problemDocuments)
		{
			if(problemDocuments == null || problemDocuments.Count == 0)
			{
				return "Детали ошибки отправки недоступны";
			}

			var parts = new List<string>();
			foreach(var document in problemDocuments)
			{
				var failureMessage = string.IsNullOrWhiteSpace(document.FailureMessage)
					? "описание ошибки отсутствует"
					: document.FailureMessage;

				parts.Add($"Чек {document.DocumentNumber}: {failureMessage}");
			}

			return string.Join("; ", parts);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
