using EdoService.Library;
using QS.Commands;
using QS.Dialog;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderDocumentActionsViewModel : WidgetViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IEdoService _edoService;
		private EdoInOrderDocumentHistoryRowViewModel _selectedDocument;
		private IEnumerable<NamedCommand> _actions = Enumerable.Empty<NamedCommand>();

		public EdoInOrderDocumentActionsViewModel(
			IInteractiveService interactiveService,
			IEdoService edoService
			)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_edoService = edoService ?? throw new ArgumentNullException(nameof(edoService));
		}

		public virtual EdoInOrderDocumentHistoryRowViewModel SelectedDocument
		{
			get => _selectedDocument;
			set
			{
				if(SetField(ref _selectedDocument, value))
				{
					CreateActions();
				}
			}
		}

		public virtual IEnumerable<NamedCommand> Actions
		{
			get => _actions;
			set => SetField(ref _actions, value);
		}

		private void CreateActions()
		{
			if(_selectedDocument == null)
			{
				Actions = Enumerable.Empty<NamedCommand>();
				return;
			}

			var newActions = new List<NamedCommand>();

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
					break;
				default:
					break;
			}

			Actions = newActions;
		}

		private void CreateUpdActions(
			List<NamedCommand> newActions,
			EdoInOrderDocumentNode document
			) 
		{
			if(_edoService.CanResend(document.EdoDocumentStatus))
			{
				newActions.Add(new NamedCommand(
					"Переотправить УПД",
					() => 
					{ 
						var result = _edoService.ResendEdoDocumentForOrder(document.TaskId);
						if(result.IsSuccess)
						{
							_interactiveService.ShowMessage(ImportanceLevel.Info, "Успешно переотправлено");
						}
						else
						{
							_interactiveService.ShowMessage(ImportanceLevel.Error,
								$"Не удалось переотправить документ.\nПричины:\n - " +
								string.Join("\n - ", result.Errors.Select(x => x.Message)));
						}
					}
				));
			}
		}

		private void CreateReceiptActions(
			List<NamedCommand> newActions,
			EdoInOrderDocumentNode document
			)
		{
			if(document.TaskReceiptStage == EdoReceiptStatus.New && document.TaskStatus == EdoTaskStatus.Problem)
			{
				newActions.Add(new NamedCommand(
					"Переотправить чек",
					() =>
					{
						var result = _edoService.ResendReceiptDocument(document.TaskId).GetAwaiter().GetResult();
						if(result.IsSuccess)
						{
							_interactiveService.ShowMessage(ImportanceLevel.Info, "Успешно переотправлено");
						}
						else
						{
							_interactiveService.ShowMessage(ImportanceLevel.Error,
								$"Не удалось переотправить документ.\nПричины:\n - " +
								string.Join("\n - ", result.Errors.Select(x => x.Message)));
						}
					}
				));
			}
		}
	}
}
