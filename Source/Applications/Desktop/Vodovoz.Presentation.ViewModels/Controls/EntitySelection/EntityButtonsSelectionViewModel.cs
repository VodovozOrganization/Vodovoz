using QS.Commands;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class EntityButtonsSelectionViewModel : WindowDialogViewModelBase
	{
		private string _topMessageString;
		private bool _isCanOpenJournal;
		private string _noEntitiesMessage;

		public event EventHandler<EntitySelectedEventArgs> EntitySelected;
		public event EventHandler SelectEntityFromJournalSelected;

		public EntityButtonsSelectionViewModel(
			INavigationManager navigation,
			IDictionary<object, string> entities,
			bool isUserCanOpenJournal = false
			) : base(navigation)
		{
			Entities = entities;
			IsCanOpenJournal = isUserCanOpenJournal;

			SelectEntityCommand = new DelegateCommand<object>(SelectEntity);
			SelectEntityFromJournalCommand = new DelegateCommand(SelectEntityFromJournal, () => IsCanOpenJournal);
		}

		public string TopMessageString
		{
			get => _topMessageString;
			set => SetField( ref _topMessageString, value );
		}

		public bool IsCanOpenJournal
		{
			get => _isCanOpenJournal;
			set => SetField(ref _isCanOpenJournal, value);
		}

		public string NoEntitiesMessage
		{
			get => _noEntitiesMessage;
			set => SetField(ref _noEntitiesMessage, value);
		}

		public IDictionary<object, string> Entities { get; }

		public DelegateCommand<object> SelectEntityCommand { get; }
		public DelegateCommand SelectEntityFromJournalCommand { get; }

		private void SelectEntity(object entity)
		{
			EntitySelected?.Invoke(this, new EntitySelectedEventArgs(entity));
			CloseWindow();
		}

		private void SelectEntityFromJournal()
		{
			SelectEntityFromJournalSelected?.Invoke(this, EventArgs.Empty);
			CloseWindow();
		}

		private void CloseWindow()
		{
			Close(false, CloseSource.Self);
		}
	}
}
