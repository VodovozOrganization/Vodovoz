using QS.Commands;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public interface IEntitySelectionViewModel : INotifyPropertyChanged, IDisposable
	{
		bool DisposeViewModel { get; set; }

		#region Выбранная сущьность
		string EntityTitle { get; }
		object Entity { get; set; }
		#endregion

		#region События для внешних подписчиков
		event EventHandler Changed;
		event EventHandler ChangedByUser;
		#endregion

		#region Настройки виджета
		bool IsEditable { get; set; }
		#endregion

		#region Доступность функций View
		bool CanSelectEntity { get; }
		bool CanClearEntity { get; }
		bool CanAutoCompleteEntry { get; }
		#endregion

		#region Команды от View
		DelegateCommand OpenSelectDialogCommand { get; }
		DelegateCommand ClearEntityCommand { get; }
		#endregion

		#region Автодополнение
		int AutocompleteListSize { get; set; }
		void AutocompleteTextEdited(string text);
		string GetAutocompleteTitle(object node);
		void AutocompleteSelectNode(object node);
		event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;
		#endregion
	}

	public class AutocompleteUpdatedEventArgs : EventArgs
	{
		public IList List;

		public AutocompleteUpdatedEventArgs(IList list)
		{
			this.List = list ?? throw new ArgumentNullException(nameof(list));
		}
	}

	public class EntitySelectedEventArgs : EventArgs
	{
		public object Entity;

		public EntitySelectedEventArgs(object entity)
		{
			Entity = entity ?? throw new ArgumentNullException(nameof(entity));
		}
	}

	public interface IEntitySelectionAutocompleteSelector<TEntity>
		where TEntity : class, IDomainObject
	{
		string GetTitle(object node);
		event EventHandler<AutocompleteUpdatedEventArgs> AutocompleteLoaded;
		void LoadAutocompletion(string[] searchText, int takeCount);
		IList<TEntity> GetEntities();
	}

	public interface IEntitySelectionAdapter<TEntity>
		where TEntity : class, IDomainObject
	{
		TEntity GetEntityByNode(object node);
		EntitySelectionViewModel<TEntity> EntitySelectionViewModel { set; }
	}

	public interface IEntityJournalSelector
	{
		void OpenSelector(string dialogTitle = null);
		event EventHandler<EntitySelectedEventArgs> EntitySelected;
	}

	public class EntityJournalViewModelSelector<TEntity, TJournalViewModel> : IEntityJournalSelector
		where TEntity : IDomainObject
		where TJournalViewModel : JournalViewModelBase
	{
		protected readonly INavigationManager NavigationManager;
		protected readonly Func<ITdiTab> GetParentTab;
		protected readonly DialogViewModelBase ParentViewModel;

		/// <summary>
		/// Специальный конструктор для старых диалогов базирующихся ITdiTab
		/// </summary>
		[Obsolete("Конструктор для совместимости со старыми диалогами, в классах с ViewModel используйте другой конструктор.")]
		public EntityJournalViewModelSelector(Func<ITdiTab> getParentTab, INavigationManager navigationManager)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			GetParentTab = getParentTab ?? throw new ArgumentNullException(nameof(getParentTab));
		}

		public EntityJournalViewModelSelector(DialogViewModelBase parentViewModel, INavigationManager navigationManager)
		{
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			ParentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
		}

		public Type EntityType => typeof(TEntity);

		public event EventHandler<EntitySelectedEventArgs> EntitySelected;

		public virtual void OpenSelector(string dialogTitle = null)
		{
			IPage<TJournalViewModel> page;

			if(ParentViewModel != null)
			{
				page = NavigationManager.OpenViewModel<TJournalViewModel>(ParentViewModel, OpenPageOptions.AsSlave);
			}
			else
			{
				page = (NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<TJournalViewModel>(GetParentTab(), OpenPageOptions.AsSlave);
			}

			page.ViewModel.SelectionMode = JournalSelectionMode.Single;

			if(!string.IsNullOrEmpty(dialogTitle))
			{
				page.ViewModel.TabName = dialogTitle;
			}

			page.ViewModel.OnSelectResult -= ViewModel_OnSelectResult;
			page.ViewModel.OnSelectResult += ViewModel_OnSelectResult;
		}

		protected void ViewModel_OnSelectResult(object sender, JournalSelectedEventArgs e)
		{
			EntitySelected?.Invoke(this, new EntitySelectedEventArgs(e.SelectedObjects.First()));
		}

	}
}
