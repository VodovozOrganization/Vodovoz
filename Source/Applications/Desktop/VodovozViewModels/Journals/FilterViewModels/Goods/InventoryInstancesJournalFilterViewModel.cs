using System;
using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Goods
{
	public class InventoryInstancesJournalFilterViewModel : FilterViewModelBase<InventoryInstancesJournalFilterViewModel>, IJournalFilterViewModel
	{
		private Nomenclature _nomenclature;
		private bool? _restrictShowArchive;
		private bool _showArchive;

		public InventoryInstancesJournalFilterViewModel(
			ILifetimeScope scope,
			INavigationManager navigationManager,
			DialogViewModelBase journalTab,
			Action<InventoryInstancesJournalFilterViewModel> filterParams = null)
		{
			Scope = scope ?? throw new ArgumentNullException(nameof(scope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			JournalTab = journalTab ?? throw new ArgumentNullException(nameof(journalTab));

			Initialize();

			if(filterParams != null)
			{
				SetAndRefilterAtOnce(filterParams);
			}
		}
		
		public ILifetimeScope Scope { get; }
		public INavigationManager NavigationManager { get; }
		public DialogViewModelBase JournalTab { get; }
		public IEntityEntryViewModel NomenclatureEntryViewModel { get; private set; }

		public bool ShowArchive
		{
			get => _showArchive;
			set => UpdateFilterField(ref _showArchive, value);
		}

		public bool CanChangeShowArchive => !RestrictShowArchive.HasValue;
		
		public bool? RestrictShowArchive
		{
			get => _restrictShowArchive;
			set
			{
				if(SetField(ref _restrictShowArchive, value) && _restrictShowArchive.HasValue)
				{
					ShowArchive = _restrictShowArchive.Value;
					OnPropertyChanged(nameof(CanChangeShowArchive));
				}
			}
		}
		
		public Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => UpdateFilterField(ref _nomenclature, value);
		}
		
		public int[] ExcludedInventoryInstancesIds { get; set; }

		public string InventoryNumber { get; set; }
		public bool CanChangeNomenclature { get; set; } = true;
		public bool IsShow { get; set; }

		private void Initialize()
		{
			var builder = new CommonEEVMBuilderFactory<InventoryInstancesJournalFilterViewModel>(
				JournalTab, this, UoW, NavigationManager, Scope);

			NomenclatureEntryViewModel = builder.ForProperty(x => x.Nomenclature)
				.UseViewModelJournalAndAutocompleter<InventoryNomenclaturesJournalViewModel>()
				.Finish();
		}
	}
}
