using EdoService.Library;
using QS.Commands;
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
		private readonly IEdoService _edoService;
		private EdoInOrderDocumentHistoryRowViewModel _selectedDocument;
		private IEnumerable<NamedCommand> _actions = Enumerable.Empty<NamedCommand>();

		public EdoInOrderDocumentActionsViewModel(IEdoService edoService)
		{
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
			if(document.TaskUpdStage == DocumentEdoTaskStage.New && document.TaskStatus == EdoTaskStatus.Problem)
			{
				newActions.Add(new NamedCommand(
					"Переобработать проблему",
					() => _edoService.RehandleNewUpdDocumentWithProblem(document.TaskId)
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
					"Переобработать проблему",
					() => _edoService.RehandleNewReceiptDocumentWithProblem(document.TaskId)
				));
			}
		}
	}
}
