using EdoService.Library;
using Gamma.Binding.Core;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Settings.Edo;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoDocumentActionsFactory : IEdoDocumentActionsFactory
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IEdoService _edoService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IEdoClosedPeriodSettings _edoClosedPeriodSettings;
		private readonly IUnitOfWorkFactory _uowFactory;

		public EdoDocumentActionsFactory(
			IInteractiveService interactiveService,
			IEdoService edoService,
			ICurrentPermissionService currentPermissionService,
			IEdoClosedPeriodSettings edoClosedPeriodSettings,
			IUnitOfWorkFactory uowFactory)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_edoClosedPeriodSettings = edoClosedPeriodSettings ?? throw new ArgumentNullException(nameof(edoClosedPeriodSettings));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public IEnumerable<BusyCommand> CreateActions(
			EdoInOrderDocumentHistoryRowViewModel document,
			Action onActionCompleted)
		{
			if(document == null)
			{
				return Enumerable.Empty<BusyCommand>();
			}

			var actions = new List<BusyCommand>();
			var documentNode = document.Document;

			if(documentNode.TaskStatus == EdoTaskStatus.New)
			{
				if(CanResendNewTask(documentNode))
				{
					CreateResendNewTaskAction(actions, documentNode, onActionCompleted);
				}

				return actions;
			}

			switch(document.DocumentType)
			{
				case EdoInOrderDocumentType.Upd:
					CreateUpdActions(actions, document.Document, onActionCompleted);
					break;
				case EdoInOrderDocumentType.Receipt:
					CreateReceiptActions(actions, document.Document, onActionCompleted);
					break;
				case EdoInOrderDocumentType.SaveCode:
					CreateSaveCodeActions(actions, document.Document);
					break;
				case EdoInOrderDocumentType.Tender:
				default:
					break;
			}

			return actions;
		}

		private static bool CanResendNewTask(EdoInOrderDocumentNode document) =>
			document.TaskType == EdoTaskType.Document && document.FormalDocumentType == EdoDocumentType.UPD
			|| document.TaskType == EdoTaskType.Receipt
			|| document.TaskType == EdoTaskType.Tender
			|| document.TaskType == EdoTaskType.SaveCode;

		private void CreateResendNewTaskAction(
			List<BusyCommand> actions,
			EdoInOrderDocumentNode document,
			Action onActionCompleted)
		{
			actions.Add(new BusyCommand(
				"Переотправить",
				() =>
				{
					if(IsResendBlockedByClosedPeriod(document.TaskId))
					{
						return;
					}

					var result = _edoService.ResendNewEdoTask(document.TaskId);
					ShowResult(result);

					if(result.IsSuccess)
					{
						onActionCompleted?.Invoke();
					}
				}));
		}

		private void CreateUpdActions(
			List<BusyCommand> actions,
			EdoInOrderDocumentNode document,
			Action onActionCompleted)
		{
			if(document.EdoDocumentStatus != EdoDocumentStatus.Succeed)
			{
				actions.Add(new BusyCommand(
					"Переотправить",
					() => ResendUpd(document, onActionCompleted)
				));
			}

			if(IsDocumentCompletedWithClarification(document)
				&& _currentPermissionService.ValidatePresetPermission(EdoPermissions.CanResendEdoDocumentWithCodesFromPool))
			{
				actions.Add(new BusyCommand(
					"Переотправить с кодами из пула",
					() => ResendUpdWithCodesFromPool(document, onActionCompleted)
				));
			}
		}

		private void ResendUpd(EdoInOrderDocumentNode document, Action onActionCompleted)
		{
			if(IsResendBlockedByClosedPeriod(document.TaskId))
			{
				return;
			}

			if(IsCanResendViaOrderDocumentSendEvent(document))
			{
				ResendViaOrderDocumentSendEvent(document, onActionCompleted);
				return;
			}

			if(IsDocumentInProgressOrSent(document))
			{
				ResendUpdWithCancellation(document, onActionCompleted);
				return;
			}

			if(IsDocumentCompletedWithClarification(document))
			{
				ShowResult(_edoService.ResendEdoDocumentWithOriginalCodes(document.TaskId));
				onActionCompleted?.Invoke();
				return;
			}

			var hasDocflow = _edoService.HasDocflow(document.TaskId);
			var hasCancelledDocflow = _edoService.HasCancelledDocflow(document.TaskId);

			if(hasDocflow && !hasCancelledDocflow)
			{
				if(!_interactiveService.Question(
					"Документооборот по данному документу завершён .\n" +
					"Для переотправки необходимо аннулировать документооборот.\n" +
					"Начать процесс аннулирования?"))
				{
					return;
				}

				var cancelResult = _edoService.CancelDocflow(document.TaskId);
				if(cancelResult.IsSuccess)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Info, cancelResult.Value);
					onActionCompleted?.Invoke();
				}
				else
				{
					ShowErrorMessage(cancelResult.Errors);
				}

				return;
			}

			var result = _edoService.ResendEdoDocumentForOrder(document.TaskId);
			if(result.IsSuccess)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, result.Value);
				onActionCompleted?.Invoke();
			}
			else
			{
				ShowErrorMessage(result.Errors);
			}
		}

		private bool IsCanResendViaOrderDocumentSendEvent(EdoInOrderDocumentNode document)
		{
			return _edoService.CanResendViaEdoRequestCreatedEvent(document.TaskId);
		}

		private void ResendViaOrderDocumentSendEvent(EdoInOrderDocumentNode document, Action onActionCompleted)
		{
			var resendResult = _edoService.TryResendViaOrderDocumentSendEventAsync(document.TaskId)
				.GetAwaiter()
				.GetResult();

			ShowResult(resendResult);

			onActionCompleted?.Invoke();
		}

		private void ResendUpdWithCancellation(EdoInOrderDocumentNode document, Action onActionCompleted)
		{
			if(IsResendBlockedByClosedPeriod(document.TaskId))
			{
				return;
			}

			if(!_currentPermissionService.ValidatePresetPermission(EdoPermissions.CanResendEdoDocumentWithCancellation))
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Для переотправки УПД в статусе «В процессе» или «Отправлен» недостаточно прав.");
				return;
			}

			if(!_interactiveService.Question(
				"Текущий документооборот будет отправлен на аннулирование.\n" +
				"Клиенту будет отправлено предложение об аннулировании.\n" +
				"УПД будет переотправлен. Продолжить?"))
			{
				return;
			}

			var result = _edoService.ResendEdoDocumentWithCancellation(document.TaskId);
			ShowResult(result);

			if(result.IsSuccess)
			{
				onActionCompleted?.Invoke();
			}
		}

		private void ResendUpdWithCodesFromPool(EdoInOrderDocumentNode document, Action onActionCompleted)
		{
			if(IsResendBlockedByClosedPeriod(document.TaskId))
			{
				return;
			}

			if(!_interactiveService.Question(
				"Документ будет переотправлен с подбором новых кодов ЧЗ из пула. Продолжить?"))
			{
				return;
			}

			ShowResult(_edoService.ResendEdoDocumentForOrderWithCodesFromPool(document.TaskId));
			onActionCompleted?.Invoke();
		}

		private bool IsDocumentCompletedWithClarification(EdoInOrderDocumentNode document)
		{
			return document.EdoDocumentStatus == EdoDocumentStatus.Warning
				|| document.EdoDocumentStatus == EdoDocumentStatus.CompletedWithDivergences;
		}

		private static bool IsDocumentInProgressOrSent(EdoInOrderDocumentNode document)
		{
			return document.EdoDocumentStatus == EdoDocumentStatus.InProgress
				|| document.EdoDocumentStatus == EdoDocumentStatus.Sent;
		}

		private void CreateReceiptActions(
			List<BusyCommand> actions,
			EdoInOrderDocumentNode document,
			Action onActionCompleted)
		{
			CreateResendReceiptAction(actions, document);

			if(document.TaskReceiptStage == EdoReceiptStatus.New && document.TaskStatus == EdoTaskStatus.Problem)
			{
				actions.Add(new BusyCommand(
					"Переобработать проблему",
					() =>
					{
						if(IsResendBlockedByClosedPeriod(document.TaskId))
						{
							return;
						}

						var result = _edoService.RehandleNewReceiptDocumentWithProblem(document.TaskId);
						if(result.IsSuccess)
						{
							_interactiveService.ShowMessage(ImportanceLevel.Info, "Успешно отправлен на переобработку");
							onActionCompleted?.Invoke();
						}
						else
						{
							_interactiveService.ShowMessage(
								ImportanceLevel.Error,
								"Не удалось переобработать проблему.\nПричины:\n - " +
									string.Join("\n - ", result.Errors.Select(x => x.Message)));
						}
					}
				));
			}
		}

		private void CreateSaveCodeActions(List<BusyCommand> actions, EdoInOrderDocumentNode document)
			=> CreateResendDocumentAction(actions, document);

		private void CreateResendDocumentAction(List<BusyCommand> actions, EdoInOrderDocumentNode document)
		{
			if(document.TaskType is EdoTaskType.SaveCode)
			{
				actions.Add(new BusyCommand(
					"Переотправить",
					() =>
					{
						if(IsResendBlockedByClosedPeriod(document.TaskId))
						{
							return;
						}

						ShowResult(_edoService.TryResendUpdDocument(document.TaskId));
					}
				));
			}
		}

		private void CreateResendReceiptAction(List<BusyCommand> actions, EdoInOrderDocumentNode document)
		{
			var isReceipt = document.TaskType is EdoTaskType.Receipt;
			var receiptSavedToPool = document.TaskReceiptStage is EdoReceiptStatus.SavedToPool;

			if(isReceipt && receiptSavedToPool)
			{
				actions.Add(new BusyCommand(
					"Переотправить",
					() =>
					{
						if(IsResendBlockedByClosedPeriod(document.TaskId))
						{
							return;
						}

						ShowResult(_edoService.TryResendReceiptDocument(document.TaskId));
					}
				));
			}
		}

		private bool IsResendBlockedByClosedPeriod(int taskId)
		{
			var accountingDate = GetAccountingDate(taskId);

			if(!accountingDate.HasValue)
			{
				return false;
			}

			if(!_edoClosedPeriodSettings.IsClosedPeriod(accountingDate.Value))
			{
				return false;
			}

			if(_currentPermissionService.ValidatePresetPermission(
				BookkeeppingPermissions.CanSendEdoDocumentsForPreviousPeriods))
			{
				return false;
			}

			_interactiveService.ShowMessage(ImportanceLevel.Error,
				"Не удалось переотправить документ.\nПричина:\nДокумент в закрытом бухгалтерском периоде. "
				+ "Для переотправки обратитесь в бухгалтерию");
			return true;
		}

		private DateTime? GetAccountingDate(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var task = uow.GetById<EdoTask>(taskId);

				if(task is OrderEdoTask orderTask && orderTask.FormalEdoRequest?.Order?.DeliveryDate != null)
				{
					return orderTask.FormalEdoRequest.Order.DeliveryDate;
				}

				if(task is ReceiptEdoTask receiptTask && receiptTask.FiscalDocuments.Any())
				{
					return receiptTask.FiscalDocuments.Max(x => x.FiscalTime ?? x.CheckoutTime);
				}

				return task?.CreationTime;
			}
		}

		private void ShowErrorMessage(IEnumerable<Error> errors)		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Error,
				"Не удалось переотправить документ.\nПричины:\n - " +
					string.Join("\n - ", errors.Select(x => x.Message)));
		}

		private void ShowResult(Result<string> result)
		{
			if(result.IsFailure)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, result.Errors.First().Message);
			}
			else
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, result.Value);
			}
		}
	}
}
