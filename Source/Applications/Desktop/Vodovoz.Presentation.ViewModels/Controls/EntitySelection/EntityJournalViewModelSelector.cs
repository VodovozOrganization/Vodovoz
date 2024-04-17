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
				page = NavigationManager.OpenViewModel<TJournalViewModel>(
					ParentViewModel,
					OpenPageOptions.AsSlave,
					GetJournalViewModelConfiguration(dialogTitle));
			}
			else
			{
				page = (NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<TJournalViewModel>(
					GetParentTab(),
					OpenPageOptions.AsSlave,
					GetJournalViewModelConfiguration(dialogTitle));
			}
		}

		protected Action<TJournalViewModel> GetJournalViewModelConfiguration(string dialogTitle)
		{
			return viewModel =>
			{
				if(!string.IsNullOrEmpty(dialogTitle))
				{
					viewModel.TabName = dialogTitle;
				}
				viewModel.SelectionMode = JournalSelectionMode.Single;
				viewModel.OnSelectResult -= ViewModel_OnSelectResult;
				viewModel.OnSelectResult += ViewModel_OnSelectResult;
			};
		}

		protected void ViewModel_OnSelectResult(object sender, JournalSelectedEventArgs e)
		{
			EntitySelected?.Invoke(this, new EntitySelectedEventArgs(e.SelectedObjects.First()));
		}
	}

	public class EntityJournalViewModelSelector<TEntity, TJournalViewModel, TJournalFilterViewModel> : EntityJournalViewModelSelector<TEntity, TJournalViewModel>
		where TEntity : IDomainObject
		where TJournalViewModel : JournalViewModelBase
		where TJournalFilterViewModel : class, IJournalFilterViewModel
	{
		private readonly Action<TJournalFilterViewModel> _filterParams;
		private readonly TJournalFilterViewModel _filter;

		public EntityJournalViewModelSelector(
			DialogViewModelBase parentViewModel,
			INavigationManager navigationManager,
			TJournalFilterViewModel filter
			) : base(parentViewModel, navigationManager)
		{
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
		}

		public EntityJournalViewModelSelector(
			DialogViewModelBase parentViewModel,
			INavigationManager navigationManager,
			Action<TJournalFilterViewModel> filterParams
			) : base(parentViewModel, navigationManager)
		{
			_filterParams = filterParams ?? throw new ArgumentNullException(nameof(filterParams));
		}

		/// <summary>
		/// Специальный конструктор для старых диалогов базирующихся ITdiTab
		/// </summary>
		[Obsolete("Конструктор для совместимости со старыми диалогами, в классах с ViewModel используйте другой конструктор.")]
		public EntityJournalViewModelSelector(
			Func<ITdiTab> getParentTab,
			INavigationManager navigationManager,
			Action<TJournalFilterViewModel> filterParams
			) : base(getParentTab, navigationManager)
		{
			_filterParams = filterParams ?? throw new ArgumentNullException(nameof(filterParams));
		}

		/// <summary>
		/// Специальный конструктор для старых диалогов базирующихся ITdiTab
		/// </summary>
		[Obsolete("Конструктор для совместимости со старыми диалогами, в классах с ViewModel используйте другой конструктор.")]
		public EntityJournalViewModelSelector(
			Func<ITdiTab> getParentTab,
			INavigationManager navigationManager,
			TJournalFilterViewModel filter
			) : base(getParentTab, navigationManager)
		{
			_filter = filter ?? throw new ArgumentNullException(nameof(filter));
		}

		public override void OpenSelector(string dialogTitle = null)
		{
			IPage<TJournalViewModel> page;
			if(ParentViewModel != null)
			{
				if(_filter != null)
				{
					page = NavigationManager.OpenViewModel<TJournalViewModel, TJournalFilterViewModel>(
						ParentViewModel,
						_filter,
						OpenPageOptions.AsSlave,
						GetJournalViewModelConfiguration(dialogTitle));
				}
				else
				{
					page = NavigationManager.OpenViewModel<TJournalViewModel>(
						ParentViewModel,
						OpenPageOptions.AsSlave,
						GetJournalViewModelConfiguration(dialogTitle));
				}
			}
			else
			{
				if(_filter != null)
				{
					page = (NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<TJournalViewModel, TJournalFilterViewModel>(
						GetParentTab(),
						_filter,
						OpenPageOptions.AsSlave,
						GetJournalViewModelConfiguration(dialogTitle));
				}
				else
				{
					page = (NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<TJournalViewModel>(
						GetParentTab(),
						OpenPageOptions.AsSlave,
						GetJournalViewModelConfiguration(dialogTitle));
				}
			}

			if(page.ViewModel.JournalFilter != null)
			{
				if(page.ViewModel.JournalFilter is IJournalFilterViewModel filter)
				{
					if(_filterParams != null)
					{
						filter.SetAndRefilterAtOnce(_filterParams);
					}
				}
				else
				{
					throw new InvalidCastException($"Для установки параметров, фильтр {page.ViewModel.JournalFilter.GetType()} должен является типом {typeof(IJournalFilterViewModel)}");
				}
			}
		}
	}
}
