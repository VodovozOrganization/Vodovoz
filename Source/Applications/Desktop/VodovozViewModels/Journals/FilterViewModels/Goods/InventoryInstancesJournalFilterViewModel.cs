using System;
using System.Linq;
using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Goods
{
	public class InventoryInstancesJournalFilterViewModel : FilterViewModelBase<InventoryInstancesJournalFilterViewModel>, IJournalFilterViewModel
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

			Initialize();
			ReFilter(filterParams);
		}
		
		public ILifetimeScope Scope { get; }
		public INavigationManager NavigationManager { get; }
		public DialogViewModelBase JournalTab { get; }
		public IEntityEntryViewModel NomenclatureViewModel { get; private set; }

		public Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => UpdateFilterField(ref _nomenclature, value);
		}

		public string InventoryNumber
		{
			get => _inventoryNumber;
			set => SetField(ref _inventoryNumber, value);
		}

		public bool CanChangeNomenclature { get; set; } = true;
		public bool IsShow { get; set; }

		private void Initialize()
		{
			var builder = new CommonEEVMBuilderFactory<InventoryInstancesJournalFilterViewModel>(
				JournalTab, this, UoW, NavigationManager, Scope);

			NomenclatureViewModel = builder.ForProperty(x => x.Nomenclature)
				.UseViewModelDialog<NomenclatureViewModel>()
				.UseViewModelJournalAndAutocompleter<NomenclaturesJournalViewModel>()
				.Finish();
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
