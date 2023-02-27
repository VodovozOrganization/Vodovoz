using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Key = Gdk.Key;

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
			nomenclatureEntry.ViewModel = ViewModel.NomenclatureEntryViewModel;
			nomenclatureEntry.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeNomenclature, w => w.Sensitive)
				.InitializeFromSource();
			
			inventoryNumberEntry.Binding
				.AddBinding(ViewModel, vm => vm.InventoryNumber, w => w.Text)
				.InitializeFromSource();
			inventoryNumberEntry.KeyReleaseEvent += OnInventoryNumberEntryKeyReleaseEvent;
		}

		private void OnInventoryNumberEntryKeyReleaseEvent(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}
	}
}
