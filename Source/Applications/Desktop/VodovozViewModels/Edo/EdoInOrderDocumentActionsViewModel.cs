using Gamma.Binding.Core;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Vodovoz.ViewModels.Edo
{
	public class EdoInOrderDocumentActionsViewModel : WidgetViewModelBase
	{
		private readonly IEdoDocumentActionsFactory _actionsFactory;
		private EdoInOrderDocumentHistoryRowViewModel _selectedDocument;
		private IEnumerable<BusyCommand> _actions = Enumerable.Empty<BusyCommand>();

		public EdoInOrderDocumentActionsViewModel(IEdoDocumentActionsFactory actionsFactory)
		{
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
	}
}
