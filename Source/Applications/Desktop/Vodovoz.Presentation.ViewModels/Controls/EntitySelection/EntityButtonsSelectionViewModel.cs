using QS.Commands;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using Vodovoz.Presentation.ViewModels.Controls.EntitySelection;

namespace Vodovoz.Presentation.ViewModels.Logistic
{
	public class EntityButtonsSelectionViewModel<TEntity> : WindowDialogViewModelBase
		where TEntity : class, IDomainObject
	{
		public event EventHandler<EntitySelectedEventArgs> EntitySelected;
		public event EventHandler SelectEntityFromJournalSelected;

		public EntityButtonsSelectionViewModel(
			INavigationManager navigation,
			IList<object> entities,
			bool isUserCanOpenJournal = false
			) : base(navigation)
		{
			Entities = entities;
			IsCanOpenJournal = isUserCanOpenJournal;

			SelectEntityCommand = new DelegateCommand<TEntity>(SelectEntity);
			SelectEntityFromJournalCommand = new DelegateCommand(SelectEntityFromJournal, () => IsCanOpenJournal);
		}

		public string TopMessageString { get; set; }
		public bool IsCanOpenJournal { get; set; } = false;
		public IList<object> Entities { get; }
		public string NoEntitiesMessage { get; set; } =
			"Данные отсутствуют";

		public DelegateCommand<TEntity> SelectEntityCommand { get; }
		public DelegateCommand SelectEntityFromJournalCommand { get; }

		private void SelectEntity(TEntity entity)
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
