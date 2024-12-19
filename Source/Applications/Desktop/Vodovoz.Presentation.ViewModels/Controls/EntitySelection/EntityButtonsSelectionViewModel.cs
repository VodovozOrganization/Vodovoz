using QS.Commands;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class EntityButtonsSelectionViewModel : WindowDialogViewModelBase
	{
		public EntityButtonsSelectionViewModel(
			INavigationManager navigation,
			IList<object> entities,
			Func<SelectionDialogSettings> dialogSettingsFunc
			) : base(navigation)
		{
			Entities = entities;
			DialogSettings = dialogSettingsFunc != null ? dialogSettingsFunc.Invoke() : new SelectionDialogSettings();

			SelectEntityCommand = new DelegateCommand<object>(SelectEntity);
			SelectEntityFromJournalCommand = new DelegateCommand(SelectEntityFromJournal, () => DialogSettings.IsCanOpenJournal);

			WindowPosition = QS.Dialog.WindowGravity.None;

			Title = DialogSettings.Title;
		}

		public event EventHandler<EntitySelectedEventArgs> EntitySelected;
		public event EventHandler SelectEntityFromJournalSelected;

		public IList<object> Entities { get; }
		public SelectionDialogSettings DialogSettings { get; }
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
