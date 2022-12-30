using System;
using System.Linq;
using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.ViewModels.Dialog;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Goods
{
	public class InventoryInstancesJournalFilterViewModel : FilterViewModelBase<InventoryInstancesJournalFilterViewModel>
	{
		public InventoryInstancesJournalFilterViewModel(
			ILifetimeScope scope,
			INavigationManager navigationManager,
			DialogViewModelBase journalTab,
			params Action<InventoryInstancesJournalFilterViewModel>[] filterParams)
		{
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			JournalTab = journalTab ?? throw new ArgumentNullException(nameof(journalTab));
			
			ReFilter(filterParams);
		}
		
		public ILifetimeScope Scope { get; }
		public INavigationManager NavigationManager { get; }
		public DialogViewModelBase JournalTab { get; }
		
		private void ReFilter(Action<InventoryInstancesJournalFilterViewModel>[] filterParams)
		{
			if(filterParams.Any())
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}
	}
}
