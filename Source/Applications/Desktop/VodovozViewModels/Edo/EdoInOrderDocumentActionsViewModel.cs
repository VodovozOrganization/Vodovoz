using EdoService.Library;
using Gamma.Binding.Core;
using QS.Dialog;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Core.Domain.Results;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderDocumentActionsViewModel : WidgetViewModelBase
	{
		private readonly IEdoDocumentActionsFactory _actionsFactory;
		private readonly IInteractiveService _interactiveService;
		private readonly IEdoService _edoService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private EdoInOrderDocumentHistoryRowViewModel _selectedDocument;
		private IEnumerable<BusyCommand> _actions = Enumerable.Empty<BusyCommand>();

		public EdoInOrderDocumentActionsViewModel(
			IInteractiveService interactiveService,
			IEdoService edoService,
			ICurrentPermissionService currentPermissionService,
			IEdoDocumentActionsFactory actionsFactory)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_actionsFactory = actionsFactory ?? throw new ArgumentNullException(nameof(actionsFactory));
		}

		public ICommand EdoInOrderRefreshCommand { get; set; }

		public virtual EdoInOrderDocumentHistoryRowViewModel SelectedDocument
		{
			get => _selectedDocument;
			set
			{
				if(SetField(ref _selectedDocument, value))
				{
					Actions = _actionsFactory.CreateActions(
						_selectedDocument,
						() => EdoInOrderRefreshCommand?.Execute(null));
				}
			}
		}

		public virtual IEnumerable<BusyCommand> Actions
		{
			get => _actions;
			set => SetField(ref _actions, value);
		}

		private void CreateActions()
		{
			if(_selectedDocument == null)
			{
				Actions = Enumerable.Empty<BusyCommand>();
				return;
			}

			var newActions = new List<BusyCommand>();

			switch(_selectedDocument.DocumentType)
			{
				case EdoInOrderDocumentType.Upd:
					CreateUpdActions(newActions, SelectedDocument.Document);
					break;
				case EdoInOrderDocumentType.Receipt:
					CreateReceiptActions(newActions, SelectedDocument.Document);
					break;
				case EdoInOrderDocumentType.Tender:
					break;
				case EdoInOrderDocumentType.SaveCode:
					CreateSaveCodeActions(newActions, SelectedDocument.Document);
					break;
				default:
					break;
			}

			Actions = newActions;
		}

		private void CreateUpdActions(
			List<BusyCommand> newActions,
			EdoInOrderDocumentNode document
			) 
		{
			newActions.Add(new BusyCommand(
				"Переотправить",
				() =>
				{
					if(IsDocumentCompletedWithClarification(document))
					{
						ShowResult(_edoService.ResendEdoDocumentWithOriginalCodes(document.TaskId));
						EdoInOrderRefreshCommand?.Execute(null);
						return;
					}

					var hasDocflow = _edoService.HasDocflow(document.TaskId);
					var hasCancelledDocflow = _edoService.HasCancelledDocflow(document.TaskId);
					if(hasDocflow && !hasCancelledDocflow)
					{
						if(_interactiveService.Question(
							"Документооборот по данному документу завершён .\n" +
							"Для переотправки необходимо аннулировать документооборот.\n" +
							"Начать процесс аннулирования?"
						))
						{
							var result = _edoService.CancelDocflow(document.TaskId);
							if(result.IsSuccess)
							{
								_interactiveService.ShowMessage(ImportanceLevel.Info, result.Value);
								EdoInOrderRefreshCommand?.Execute(null);
							}
							else
							{
								ShowErrorMessage(result.Errors);
							}
						}
						else
						{
							return;
						}
					}
					else
					{
						var result = _edoService.ResendEdoDocumentForOrder(document.TaskId);
						if(result.IsSuccess)
						{
							_interactiveService.ShowMessage(ImportanceLevel.Info, result.Value);
							EdoInOrderRefreshCommand?.Execute(null);
						}
						else
						{
							ShowErrorMessage(result.Errors);
						}
					}
				}
			));

			if(IsDocumentCompletedWithClarification(document)
				&& _currentPermissionService.ValidatePresetPermission(EdoPermissions.CanResendEdoDocumentWithCodesFromPool))
			{
				newActions.Add(new BusyCommand(
					"Переотправить с кодами из пула",
					() =>
					{
						if(!_interactiveService.Question(
							"Документ будет переотправлен с подбором новых кодов ЧЗ из пула. Продолжить?"))
						{
							return;
						}

						ShowResult(_edoService.ResendEdoDocumentForOrderWithCodesFromPool(document.TaskId));
						EdoInOrderRefreshCommand?.Execute(null);
					}
				));
			}
		}

		private bool IsDocumentCompletedWithClarification(EdoInOrderDocumentNode document)
		{
			return document.EdoDocumentStatus == EdoDocumentStatus.Warning
				|| document.EdoDocumentStatus == EdoDocumentStatus.CompletedWithDivergences;
		}

		private void CreateReceiptActions(
			List<BusyCommand> newActions,
			EdoInOrderDocumentNode document
			)
		{
			CreateResendReceiptAction(newActions, document);

			if(document.TaskReceiptStage == EdoReceiptStatus.New && document.TaskStatus == EdoTaskStatus.Problem)
			{
				newActions.Add(new BusyCommand(
					"Переобработать проблему",
					() => {
						var result = _edoService.RehandleNewReceiptDocumentWithProblem(document.TaskId);
						if(result.IsSuccess)
						{
							_interactiveService.ShowMessage(ImportanceLevel.Info, "Успешно отправлен на переобработку");
							EdoInOrderRefreshCommand?.Execute(null);
						}
						else
						{
							_interactiveService.ShowMessage(ImportanceLevel.Error,
								$"Не удалось переобработать проблему.\nПричины:\n - " +
								string.Join("\n - ", result.Errors.Select(x => x.Message)));
						}
					}
				));
			}
		}

		private void ShowErrorMessage(IEnumerable<Error> errors)
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Error,
				$"Не удалось переотправить документ.\nПричины:\n - " +
					string.Join("\n - ", errors.Select(x => x.Message)));
		}

		private void CreateSaveCodeActions(
			List<BusyCommand> newActions,
			EdoInOrderDocumentNode document
			)
		{
			CreateResendDocumentAction(newActions, document);
		}

		private void CreateResendDocumentAction(
			List<BusyCommand> newActions,
			EdoInOrderDocumentNode document
			)
		{
			if(document.TaskType is EdoTaskType.SaveCode)
			{
				newActions.Add(new BusyCommand(
					"Переотправить",
					() => ShowResult(_edoService.TryResendUpdDocument(document.TaskId))
				));
			}
		}

		private void CreateResendReceiptAction(
			List<BusyCommand> newActions,
			EdoInOrderDocumentNode document
			)
		{
			var isReceipt = document.TaskType is EdoTaskType.Receipt;
			var receiptSavedToPool = document.TaskReceiptStage is EdoReceiptStatus.SavedToPool;

			if(isReceipt && receiptSavedToPool)
			{
				newActions.Add(new BusyCommand(
					"Переотправить",
					() => ShowResult(_edoService.TryResendReceiptDocument(document.TaskId))
				));
			}
		}

		private void ShowResult(Result<string> result)
		{
			if(result.IsFailure)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					result.Errors.First().Message
				);
			}
			else
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Info,
					result.Value
				);
			}
		}
	}
}
