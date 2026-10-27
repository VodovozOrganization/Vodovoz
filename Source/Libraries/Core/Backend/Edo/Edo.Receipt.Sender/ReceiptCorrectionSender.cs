using Edo.Common;
using Edo.Contracts.Messages.Events;
using Edo.Transport;
using MassTransit;
using Microsoft.Extensions.Logging;
using ModulKassa;
using ModulKassa.DTO;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Application.Receipts.Correction;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Receipts;
using Vodovoz.Settings.Edo;
using EdoFiscalDocumentStatus = Vodovoz.Core.Domain.Edo.FiscalDocumentStatus;

namespace Edo.Receipt.Sender
{
	public class ReceiptCorrectionSender
	{
		private readonly ILogger<ReceiptCorrectionSender> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IReceiptCorrectionRepository _receiptCorrectionRepository;
		private readonly ReceiptCorrectionFiscalDocumentBuilder _fiscalDocumentBuilder;
		private readonly FiscalDocumentFactory _fiscalDocumentFactory;
		private readonly CashboxClientProvider _cashboxClientProvider;
		private readonly IEdoReceiptSettings _edoReceiptSettings;
		private readonly ReceiptCorrectionSaleCodesAssigner _saleCodesAssigner;
		private readonly ReceiptCorrectionIndustryRequisitePreparer _industryRequisitePreparer;
		private readonly ReceiptCorrectionCodeCirculationGuard _codeCirculationGuard;
		private readonly IBus _messageBus;
		private readonly ReceiptCorrectionCashlessDocumentRequestCreator _cashlessDocumentRequestCreator;

		public ReceiptCorrectionSender(
			ILogger<ReceiptCorrectionSender> logger,
			IUnitOfWorkFactory uowFactory,
			IReceiptCorrectionRepository receiptCorrectionRepository,
			ReceiptCorrectionFiscalDocumentBuilder fiscalDocumentBuilder,
			FiscalDocumentFactory fiscalDocumentFactory,
			CashboxClientProvider cashboxClientProvider,
			IEdoReceiptSettings edoReceiptSettings,
			ReceiptCorrectionSaleCodesAssigner saleCodesAssigner,
			ReceiptCorrectionIndustryRequisitePreparer industryRequisitePreparer,
			ReceiptCorrectionCodeCirculationGuard codeCirculationGuard,
			IBus messageBus,
			ReceiptCorrectionCashlessDocumentRequestCreator cashlessDocumentRequestCreator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_receiptCorrectionRepository = receiptCorrectionRepository
				?? throw new ArgumentNullException(nameof(receiptCorrectionRepository));
			_fiscalDocumentBuilder = fiscalDocumentBuilder
				?? throw new ArgumentNullException(nameof(fiscalDocumentBuilder));
			_fiscalDocumentFactory = fiscalDocumentFactory
				?? throw new ArgumentNullException(nameof(fiscalDocumentFactory));
			_cashboxClientProvider = cashboxClientProvider
				?? throw new ArgumentNullException(nameof(cashboxClientProvider));
			_edoReceiptSettings = edoReceiptSettings
				?? throw new ArgumentNullException(nameof(edoReceiptSettings));
			_saleCodesAssigner = saleCodesAssigner
				?? throw new ArgumentNullException(nameof(saleCodesAssigner));
			_industryRequisitePreparer = industryRequisitePreparer
				?? throw new ArgumentNullException(nameof(industryRequisitePreparer));
			_codeCirculationGuard = codeCirculationGuard
				?? throw new ArgumentNullException(nameof(codeCirculationGuard));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_cashlessDocumentRequestCreator = cashlessDocumentRequestCreator
				?? throw new ArgumentNullException(nameof(cashlessDocumentRequestCreator));
		}

		public async Task ProcessActiveCorrections(CancellationToken cancellationToken)
		{
			int[] processIds;
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				processIds = _receiptCorrectionRepository.GetActiveProcessIds(uow).ToArray();
			}

			foreach(var processId in processIds)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					return;
				}

				try
				{
					await ProcessOne(processId, cancellationToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка обработки процесса корректировки чека {ProcessId}", processId);
					await MarkProcessFailed(processId, ex.Message, cancellationToken);
				}
			}
		}

		private async Task MarkProcessFailed(int processId, string error, CancellationToken cancellationToken)
		{
			try
			{
				using(var uow = _uowFactory.CreateWithoutRoot())
				{
					var process = uow.GetById<ReceiptCorrectionProcess>(processId);
					if(process == null
						|| process.Status == ReceiptCorrectionProcessStatus.Completed
						|| process.Status == ReceiptCorrectionProcessStatus.Failed)
					{
						return;
					}

					process.Status = ReceiptCorrectionProcessStatus.Failed;
					process.ErrorDescription = error;
					process.CompletedDate = DateTime.Now;
					await uow.SaveAsync(process, cancellationToken: cancellationToken);
					await uow.CommitAsync(cancellationToken);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Не удалось зафиксировать ошибку процесса корректировки чека {ProcessId}", processId);
			}
		}

		private static bool IsDeferredUntilTransfer(FiscalDocumentType documentType)
		{
			return documentType == FiscalDocumentType.Sale
				|| documentType == FiscalDocumentType.SaleCorrection;
		}

		private static bool HasTransferInProgress(IUnitOfWork uow, ReceiptCorrectionProcess process)
		{
			if(!process.ReceiptEdoTaskId.HasValue)
			{
				return false;
			}

			var task = uow.GetById<ReceiptEdoTask>(process.ReceiptEdoTaskId.Value);
			return task?.TransferIterations?.Any(x => x.Status == TransferEdoRequestIterationStatus.InProgress) == true;
		}

		private async Task ProcessOne(int processId, CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var process = uow.GetById<ReceiptCorrectionProcess>(processId);
				if(process == null)
				{
					_logger.LogWarning("Процесс корректировки чека {ProcessId} не найден.", processId);
					return;
				}

				if(process.Status != ReceiptCorrectionProcessStatus.Pending
					&& process.Status != ReceiptCorrectionProcessStatus.InProgress)
				{
					return;
				}

				await RefreshInProgressDocuments(uow, process, cancellationToken);
				if(TryFinishProcess(process))
				{
					var finishedRequestId = await FinishCorrectionProcess(uow, process, reconcileWhenFailed: true, cancellationToken);
					await MarkCorrectionReceiptTask(uow, process, cancellationToken);
					await SaveProcess(uow, process, cancellationToken);
					await PublishCashlessDocumentRequest(finishedRequestId, cancellationToken);
					return;
				}

				if(process.Documents.Any(x => x.Status == ReceiptCorrectionProcessStatus.InProgress))
				{
					await MarkCorrectionReceiptTask(uow, process, cancellationToken);
					await SaveProcess(uow, process, cancellationToken);
					return;
				}

				var nextDocument = process.Documents
					.Where(x => x.Status == ReceiptCorrectionProcessStatus.Pending)
					.OrderBy(x => x.PlannedDocumentType == FiscalDocumentType.SaleReturnCorrection ? 0 : 1)
					.ThenBy(x => x.Id)
					.FirstOrDefault();

				if(nextDocument == null)
				{
					TryFinishProcess(process);
					var finishedRequestId = await FinishCorrectionProcess(uow, process, reconcileWhenFailed: true, cancellationToken);
					await MarkCorrectionReceiptTask(uow, process, cancellationToken);
					await SaveProcess(uow, process, cancellationToken);
					await PublishCashlessDocumentRequest(finishedRequestId, cancellationToken);
					return;
				}

				if(IsDeferredUntilTransfer(nextDocument.PlannedDocumentType)
					&& HasTransferInProgress(uow, process))
				{
					_logger.LogInformation(
						"Приход корректировки процесса {ProcessId} ждёт завершения трансфера кодов в Честный знак.",
						process.Id);
					return;
				}

				if(ReceiptSendPauseTimeHelper.IsNightPauseTime(
					DateTime.Now.TimeOfDay,
					_edoReceiptSettings.ReceiptSendPauseStartTime,
					_edoReceiptSettings.ReceiptSendPauseEndTime))
				{
					_logger.LogInformation(
						"Отправка корректирующего чека процесса {ProcessId} отложена из-за ночного окна.",
						process.Id);
					return;
				}

				var transferIterationId = await SendDocument(uow, process, nextDocument, cancellationToken);
				var cashlessRequestId = await FinishCorrectionProcess(
					uow,
					process,
					reconcileWhenFailed: false,
					cancellationToken);
				await MarkCorrectionReceiptTask(uow, process, cancellationToken);
				await SaveProcess(uow, process, cancellationToken);
				if(transferIterationId.HasValue)
				{
					await _messageBus.Publish(new TransferRequestCreatedEvent
					{
						TransferIterationId = transferIterationId.Value
					});
				}

				await PublishCashlessDocumentRequest(cashlessRequestId, cancellationToken);
			}
		}

		private async Task RefreshInProgressDocuments(
			IUnitOfWork uow,
			ReceiptCorrectionProcess process,
			CancellationToken cancellationToken)
		{
			foreach(var processDocument in process.Documents
				.Where(x => x.Status == ReceiptCorrectionProcessStatus.InProgress
					|| x.Status == ReceiptCorrectionProcessStatus.Pending))
			{
				var edoDocument = ResolveProcessEdoDocument(uow, processDocument);
				if(edoDocument == null)
				{
					continue;
				}

				if(!processDocument.EdoFiscalDocumentId.HasValue)
				{
					processDocument.EdoFiscalDocumentId = edoDocument.Id;
				}

				if(IsFiscalizationFinished(edoDocument))
				{
					CompleteProcessDocument(processDocument, edoDocument);
					continue;
				}

				if(processDocument.Status != ReceiptCorrectionProcessStatus.InProgress)
				{
					continue;
				}

				var cashboxId = GetCashboxId(edoDocument) ?? ResolveProcessCashboxId(uow, process);
				if(cashboxId == null)
				{
					continue;
				}

				var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(cashboxId.Value, cancellationToken);
				var result = await cashboxClient.CheckFiscalDocument(edoDocument.DocumentGuid.ToString(), cancellationToken);
				if(result.SendStatus == SendStatus.Error || result.FiscalDocumentInfo == null)
				{
					_logger.LogWarning(
						"Не удалось проверить статус корректирующего чека {DocumentGuid}: {Error}",
						edoDocument.DocumentGuid,
						result.ErrorMessage);
					continue;
				}

				ApplyFiscalizationResult(edoDocument, result);

				if(edoDocument.Status == EdoFiscalDocumentStatus.Failed)
				{
					FailProcess(process, processDocument, edoDocument.FailureMessage ?? result.ErrorMessage);
					continue;
				}

				if(IsFiscalizationFinished(edoDocument))
				{
					CompleteProcessDocument(processDocument, edoDocument);
				}

				await uow.SaveAsync(edoDocument, cancellationToken: cancellationToken);
			}
		}

		private static EdoFiscalDocument ResolveProcessEdoDocument(
			IUnitOfWork uow,
			ReceiptCorrectionProcessDocument processDocument)
		{
			if(processDocument.EdoFiscalDocumentId.HasValue)
			{
				var byId = uow.GetById<EdoFiscalDocument>(processDocument.EdoFiscalDocumentId.Value);
				if(byId != null)
				{
					return byId;
				}
			}

			return uow.Session.QueryOver<EdoFiscalDocument>()
				.Where(x => x.DocumentGuid == processDocument.DocumentGuid)
				.SingleOrDefault();
		}

		private async Task<int?> SendDocument(
			IUnitOfWork uow,
			ReceiptCorrectionProcess process,
			ReceiptCorrectionProcessDocument processDocument,
			CancellationToken cancellationToken)
		{
			var sourceDocumentId = process.SourceEdoFiscalDocumentId;
			if(sourceDocumentId == null)
			{
				FailProcess(process, processDocument, "Не найден исходный фискальный документ для корректировки.");
				return null;
			}

			var sourceDocument = uow.GetById<EdoFiscalDocument>(sourceDocumentId.Value);
			if(sourceDocument == null)
			{
				FailProcess(process, processDocument, $"Исходный фискальный документ #{sourceDocumentId} не найден.");
				return null;
			}

			var cashboxId = GetCashboxId(sourceDocument);
			if(cashboxId == null)
			{
				FailProcess(process, processDocument, "Не указана касса для отправки корректирующего чека.");
				return null;
			}

			EdoFiscalDocument edoFiscalDocument;
			if(processDocument.EdoFiscalDocumentId.HasValue)
			{
				edoFiscalDocument = uow.GetById<EdoFiscalDocument>(processDocument.EdoFiscalDocumentId.Value);
			}
			else
			{
				var correctionTask = ResolveCorrectionTask(uow, process, sourceDocument);
				edoFiscalDocument = _fiscalDocumentBuilder.CreateFromSource(
					sourceDocument,
					correctionTask,
					process,
					processDocument,
					currentOrder: correctionTask?.FormalEdoRequest?.Order);
				await uow.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
				processDocument.EdoFiscalDocumentId = edoFiscalDocument.Id;
			}

			if(edoFiscalDocument == null)
			{
				FailProcess(process, processDocument, "Не удалось создать фискальный документ корректировки.");
				return null;
			}

			await _saleCodesAssigner.AssignAsync(uow, edoFiscalDocument, sourceDocument, cancellationToken);

			var returnMarkError = _codeCirculationGuard.GetReturnWithoutMarkError(edoFiscalDocument);
			if(returnMarkError != null)
			{
				edoFiscalDocument.FailureMessage = returnMarkError;
				edoFiscalDocument.Status = EdoFiscalDocumentStatus.SendError;
				FailProcess(process, processDocument, returnMarkError);
				await uow.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
				return null;
			}

			if(IsDeferredUntilTransfer(edoFiscalDocument.DocumentType))
			{
				var circulation = await _codeCirculationGuard.EnsureSaleCanBeSent(
					uow,
					edoFiscalDocument,
					cashboxId.Value,
					cancellationToken);
				if(circulation.Decision == ReceiptCorrectionCirculationDecision.Wait)
				{
					process.ErrorDescription = circulation.Message;
					_logger.LogInformation(
						"Приход корректировки процесса {ProcessId} не отправлен: {Reason}",
						process.Id,
						circulation.Message);
					return circulation.TransferIterationId;
				}

				if(circulation.Decision == ReceiptCorrectionCirculationDecision.Reject)
				{
					edoFiscalDocument.FailureMessage = circulation.Message;
					edoFiscalDocument.Status = EdoFiscalDocumentStatus.SendError;
					FailProcess(process, processDocument, circulation.Message);
					await uow.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
					return null;
				}

				process.ErrorDescription = null;
			}

			var industryRequisiteError = await _industryRequisitePreparer.PrepareAsync(
				uow,
				edoFiscalDocument,
				cancellationToken);
			if(industryRequisiteError != null)
			{
				edoFiscalDocument.FailureMessage = industryRequisiteError;
				edoFiscalDocument.Status = EdoFiscalDocumentStatus.SendError;
				FailProcess(process, processDocument, industryRequisiteError);
				await uow.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
				return null;
			}

			await uow.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);

			var cashboxClient = await _cashboxClientProvider.GetCashboxAsync(cashboxId.Value, cancellationToken);
			var fiscalDocument = _fiscalDocumentFactory.CreateFiscalDocument(edoFiscalDocument, sourceDocument);

			var result = await cashboxClient.CheckFiscalDocument(fiscalDocument, cancellationToken);
			if(result.SendStatus == SendStatus.Error)
			{
				result = await cashboxClient.SendFiscalDocument(fiscalDocument, cancellationToken);
			}

			if(result.SendStatus == SendStatus.Error)
			{
				edoFiscalDocument.FailureMessage = result.ErrorMessage;
				edoFiscalDocument.Status = EdoFiscalDocumentStatus.SendError;
				FailProcess(process, processDocument, result.ErrorMessage);
				await uow.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
				return null;
			}

			ApplyFiscalizationResult(edoFiscalDocument, result);
			process.Status = ReceiptCorrectionProcessStatus.InProgress;
			processDocument.Status = ReceiptCorrectionProcessStatus.InProgress;
			processDocument.ErrorDescription = null;

			if(edoFiscalDocument.Status == EdoFiscalDocumentStatus.Failed)
			{
				FailProcess(process, processDocument, edoFiscalDocument.FailureMessage ?? result.ErrorMessage);
			}
			else if(IsFiscalizationFinished(edoFiscalDocument))
			{
				CompleteProcessDocument(processDocument, edoFiscalDocument);
				TryFinishProcess(process);
			}

			await uow.SaveAsync(edoFiscalDocument, cancellationToken: cancellationToken);
			_logger.LogInformation(
				"Корректирующий чек {DocumentType} процесса {ProcessId} отправлен, GUID {DocumentGuid}.",
				processDocument.PlannedDocumentType,
				process.Id,
				processDocument.DocumentGuid);
			return null;
		}

		private static void ApplyFiscalizationResult(EdoFiscalDocument edoFiscalDocument, FiscalizationResult result)
		{
			edoFiscalDocument.Stage = FiscalDocumentStage.Sent;
			edoFiscalDocument.Status = ReceiptConverters.ConvertFiscalDocumentStatus(result.FiscalDocumentInfo.Status);
			if(!string.IsNullOrWhiteSpace(result.FiscalDocumentInfo.TimeStatusChangedString)
				&& DateTime.TryParse(result.FiscalDocumentInfo.TimeStatusChangedString, out var statusChangeTime))
			{
				edoFiscalDocument.StatusChangeTime = statusChangeTime;
			}

			if(result.FiscalDocumentInfo.FiscalInfo != null)
			{
				if(!string.IsNullOrWhiteSpace(result.FiscalDocumentInfo.FiscalInfo.Date)
					&& DateTime.TryParse(result.FiscalDocumentInfo.FiscalInfo.Date, out var fiscalTime))
				{
					edoFiscalDocument.FiscalTime = fiscalTime;
				}

				edoFiscalDocument.FiscalNumber = result.FiscalDocumentInfo.FiscalInfo.FnDocNumber.ToString();
				edoFiscalDocument.FiscalMark = result.FiscalDocumentInfo.FiscalInfo.FnDocMark.ToString();
				edoFiscalDocument.FiscalKktNumber = result.FiscalDocumentInfo.FiscalInfo.KktNumber;
			}

			if(edoFiscalDocument.Status == EdoFiscalDocumentStatus.Completed
				|| edoFiscalDocument.Status == EdoFiscalDocumentStatus.Printed)
			{
				edoFiscalDocument.Stage = FiscalDocumentStage.Completed;
			}

			if(!string.IsNullOrWhiteSpace(result.ErrorMessage)
				&& edoFiscalDocument.Status == EdoFiscalDocumentStatus.Failed)
			{
				edoFiscalDocument.FailureMessage = result.ErrorMessage;
			}
		}

		private static bool IsFiscalizationFinished(EdoFiscalDocument document)
		{
			return document.Stage == FiscalDocumentStage.Completed
				|| document.Status == EdoFiscalDocumentStatus.Completed
				|| document.Status == EdoFiscalDocumentStatus.Printed;
		}

		private static int? GetCashboxId(EdoFiscalDocument document)
		{
			return document.ReceiptEdoTask?.CashboxId;
		}

		private static int? ResolveProcessCashboxId(IUnitOfWork uow, ReceiptCorrectionProcess process)
		{
			if(!process.SourceEdoFiscalDocumentId.HasValue)
			{
				return null;
			}

			var sourceDocument = uow.GetById<EdoFiscalDocument>(process.SourceEdoFiscalDocumentId.Value);
			return sourceDocument == null ? null : GetCashboxId(sourceDocument);
		}

		private async Task MarkCorrectionReceiptTask(
			IUnitOfWork uow,
			ReceiptCorrectionProcess process,
			CancellationToken cancellationToken)
		{
			if(!process.ReceiptEdoTaskId.HasValue)
			{
				return;
			}

			var task = uow.GetById<ReceiptEdoTask>(process.ReceiptEdoTaskId.Value);
			if(task == null)
			{
				return;
			}

			var changed = false;
			var cashboxId = task.CashboxId ?? ResolveProcessCashboxId(uow, process);
			if(!task.CashboxId.HasValue && cashboxId.HasValue)
			{
				task.CashboxId = cashboxId;
				changed = true;
			}

			if(process.Status == ReceiptCorrectionProcessStatus.Completed)
			{
				if(task.Status != EdoTaskStatus.Completed)
				{
					task.Status = EdoTaskStatus.Completed;
					changed = true;
				}

				if(task.ReceiptStatus != EdoReceiptStatus.Completed)
				{
					task.ReceiptStatus = EdoReceiptStatus.Completed;
					changed = true;
				}
			}
			else if(process.Status == ReceiptCorrectionProcessStatus.InProgress
				|| process.Documents.Any(x =>
					x.Status == ReceiptCorrectionProcessStatus.InProgress
					|| x.Status == ReceiptCorrectionProcessStatus.Completed))
			{
				if(task.Status == EdoTaskStatus.New || task.Status == EdoTaskStatus.Waiting)
				{
					task.Status = EdoTaskStatus.InProgress;
					changed = true;
				}

				if(task.ReceiptStatus == EdoReceiptStatus.New
					|| task.ReceiptStatus == EdoReceiptStatus.SavedToPool
					|| task.ReceiptStatus == EdoReceiptStatus.Transfering
					|| task.ReceiptStatus == EdoReceiptStatus.Sending)
				{
					task.ReceiptStatus = EdoReceiptStatus.Sent;
					changed = true;
				}
			}

			if(!changed)
			{
				return;
			}

			await uow.SaveAsync(task, cancellationToken: cancellationToken);
			_logger.LogInformation(
				"Задача корректирующего чека {TaskId} переведена в статус {TaskStatus}, стадия {ReceiptStatus}.",
				task.Id,
				task.Status,
				task.ReceiptStatus);
		}

		private static ReceiptEdoTask ResolveCorrectionTask(
			IUnitOfWork uow,
			ReceiptCorrectionProcess process,
			EdoFiscalDocument sourceDocument)
		{
			if(process.ReceiptEdoTaskId.HasValue)
			{
				var correctionTask = uow.GetById<ReceiptEdoTask>(process.ReceiptEdoTaskId.Value);
				if(correctionTask != null)
				{
					return correctionTask;
				}
			}

			return sourceDocument.ReceiptEdoTask;
		}

		private static void CompleteProcessDocument(
			ReceiptCorrectionProcessDocument processDocument,
			EdoFiscalDocument edoDocument)
		{
			processDocument.Status = ReceiptCorrectionProcessStatus.Completed;
			processDocument.EdoFiscalDocumentId = edoDocument.Id;
			processDocument.ErrorDescription = null;
		}

		private static void FailProcess(
			ReceiptCorrectionProcess process,
			ReceiptCorrectionProcessDocument processDocument,
			string error)
		{
			processDocument.Status = ReceiptCorrectionProcessStatus.Failed;
			processDocument.ErrorDescription = error;
			process.Status = ReceiptCorrectionProcessStatus.Failed;
			process.ErrorDescription = error;
			process.CompletedDate = DateTime.Now;
		}

		private static bool TryFinishProcess(ReceiptCorrectionProcess process)
		{
			if(process.Documents.Any(x => x.Status == ReceiptCorrectionProcessStatus.Failed))
			{
				process.Status = ReceiptCorrectionProcessStatus.Failed;
				process.CompletedDate = process.CompletedDate ?? DateTime.Now;
				return true;
			}

			if(process.Documents.Any()
				&& process.Documents.All(x => x.Status == ReceiptCorrectionProcessStatus.Completed))
			{
				process.Status = ReceiptCorrectionProcessStatus.Completed;
				process.CompletedDate = DateTime.Now;
				process.ErrorDescription = null;
				return true;
			}

			return false;
		}

		private async Task<int?> FinishCorrectionProcess(
			IUnitOfWork uow,
			ReceiptCorrectionProcess process,
			bool reconcileWhenFailed,
			CancellationToken cancellationToken)
		{
			if(process.Status == ReceiptCorrectionProcessStatus.Completed
				&& _cashlessDocumentRequestCreator.IsCashlessReceiptZeroing(process))
			{
				return await _cashlessDocumentRequestCreator.TryCreateAsync(uow, process, cancellationToken);
			}

			if(process.Status == ReceiptCorrectionProcessStatus.Completed
				|| (reconcileWhenFailed && process.Status == ReceiptCorrectionProcessStatus.Failed))
			{
				await ReconcileProcessOrderPoolCodesAsync(uow, process, cancellationToken);
			}

			return null;
		}

		private async Task PublishCashlessDocumentRequest(int? requestId, CancellationToken cancellationToken)
		{
			if(!requestId.HasValue)
			{
				return;
			}

			try
			{
				await _cashlessDocumentRequestCreator.PublishAsync(requestId.Value, cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Чек коррекции уже занулён, но заявка ЭДО {RequestId} не ушла в планировщик.",
					requestId.Value);
			}
		}

		private async Task ReconcileProcessOrderPoolCodesAsync(
			IUnitOfWork uow,
			ReceiptCorrectionProcess process,
			CancellationToken cancellationToken)
		{
			var order = uow.GetById<OrderEntity>(process.OrderId);
			if(order == null)
			{
				return;
			}

			await _saleCodesAssigner.ReconcileOrderPoolCodesAsync(uow, order, cancellationToken);
		}

		private static async Task SaveProcess(
			IUnitOfWork uow,
			ReceiptCorrectionProcess process,
			CancellationToken cancellationToken)
		{
			foreach(var document in process.Documents)
			{
				await uow.SaveAsync(document, cancellationToken: cancellationToken);
			}

			await uow.SaveAsync(process, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);
		}
	}
}
