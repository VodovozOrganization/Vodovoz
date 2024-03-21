using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Project.Journal;
using QS.Tdi;
using QS.ViewModels.Dialog;
using System;
using System.Linq;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
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
