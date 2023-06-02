using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Key = Gdk.Key;

namespace Vodovoz.Filters.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class InventoryInstancesStockBalanceJournalFilterView :
		FilterViewBase<InventoryInstancesStockBalanceJournalFilterViewModel>
	{
		public InventoryInstancesStockBalanceJournalFilterView(
			InventoryInstancesStockBalanceJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			enumCmbStorageType.ItemsEnum = typeof(StorageType);
			enumCmbStorageType.ShowSpecialStateAll = true;
			enumCmbStorageType.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StorageType, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.CanChangeStorageType, w => w.Sensitive)
				.InitializeFromSource();
			
			lblStorage.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowStorage, w => w.Visible)
				.AddBinding(vm => vm.StorageLabel, w => w.LabelProp)
				.InitializeFromSource();

			warehouseEntry.ViewModel = ViewModel.WarehouseStorageEntryViewModel;
			warehouseEntry.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowWarehouseStorage, w => w.Visible)
				.AddBinding(vm => vm.CanChangeWarehouseStorage, w => w.Sensitive)
				.InitializeFromSource();
			employeeStorageEntry.ViewModel = ViewModel.EmployeeStorageEntryViewModel;
			employeeStorageEntry.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowEmployeeStorage, w => w.Visible)
				.AddBinding(vm => vm.CanChangeEmployeeStorage, w => w.Sensitive)
				.InitializeFromSource();
			carStorageEntry.ViewModel = ViewModel.CarStorageEntryViewModel;
			carStorageEntry.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanShowCarStorage, w => w.Visible)
				.AddBinding(vm => vm.CanChangeCarStorage, w => w.Sensitive)
				.InitializeFromSource();

			nomenclatureEntry.ViewModel = ViewModel.NomenclatureEntryViewModel;
			nomenclatureEntry.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeNomenclature, w => w.Sensitive)
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
