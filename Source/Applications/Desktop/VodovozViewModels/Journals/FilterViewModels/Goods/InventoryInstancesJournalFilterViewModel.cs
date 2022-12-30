using System;
using System.Linq;
using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Goods;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Goods
{
	public class InventoryInstancesJournalFilterViewModel : FilterViewModelBase<InventoryInstancesJournalFilterViewModel>
	{
		private Nomenclature _nomenclature;
		private string _inventoryNumber;

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

		public Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => UpdateFilterField(ref _nomenclature, value);
		}

		public string InventoryNumber
		{
			get => _inventoryNumber;
			set => UpdateFilterField(ref _inventoryNumber, value);
		}

		private void ReFilter(Action<InventoryInstancesJournalFilterViewModel>[] filterParams)
		{
			if(filterParams.Any())
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}
	}
}
