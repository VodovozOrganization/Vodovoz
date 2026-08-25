using EdoService.Library;
using Gamma.Binding.Core;
using QS.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoDocumentActionsFactory : IEdoDocumentActionsFactory
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IEdoService _edoService;

		public EdoDocumentActionsFactory(
			IInteractiveService interactiveService,
			IEdoService edoService)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));
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

		private void CreateUpdActions(
			List<BusyCommand> actions,
			EdoInOrderDocumentNode document,
			Action onActionCompleted)
		{
			actions.Add(new BusyCommand(
				"Переотправить",
				() => ResendUpd(document, onActionCompleted)
			));
		}

		private void ResendUpd(EdoInOrderDocumentNode document, Action onActionCompleted)
		{
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
					() => ShowResult(_edoService.TryResendUpdDocument(document.TaskId))
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
					() => ShowResult(_edoService.TryResendReceiptDocument(document.TaskId))
				));
			}
		}

		private void ShowErrorMessage(IEnumerable<Error> errors)
		{
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
