using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz.JournalFilters.Goods
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class InventoryInstancesJournalFilterView : FilterViewBase<InventoryInstancesJournalFilterViewModel>
	{
		public InventoryInstancesJournalFilterView(InventoryInstancesJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			nomenclatureEntry.ViewModel = ViewModel.NomenclatureViewModel;
			nomenclatureEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeNomenclature, w => w.Sensitive)
				.InitializeFromSource();
			
			inventoryNumberEntry.Binding
				.AddBinding(ViewModel, vm => vm.InventoryNumber, w => w.Text)
				.InitializeFromSource();
		}
	}
}
