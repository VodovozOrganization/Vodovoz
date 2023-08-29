using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Goods;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureStockFilterView : FilterViewBase<NomenclatureStockFilterViewModel>
	{
		public NomenclatureStockFilterView(NomenclatureStockFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			warehouseEntry.ViewModel = ViewModel.WarehouseEntryViewModel;
			warehouseEntry.WidthRequest = 300;
			warehouseEntry.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeWarehouse, w => w.Sensitive)
				.InitializeFromSource();
			employeeStorageEntry.ViewModel = ViewModel.EmployeeStorageEntryViewModel;
			employeeStorageEntry.WidthRequest = 300;
			employeeStorageEntry.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeEmployeeStorage, w => w.Sensitive)
				.InitializeFromSource();
			сarStorageEntry.ViewModel = ViewModel.CarStorageEntryViewModel;
			сarStorageEntry.WidthRequest = 300;
			сarStorageEntry.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeCarStorage, w => w.Sensitive)
				.InitializeFromSource();

			checkShowArchive.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowArchive, w => w.Active)
				.AddBinding(vm => vm.CanChangeShowArchive, w => w.Sensitive)
				.InitializeFromSource();
			chkShowNomenclatureInstance.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowNomenclatureInstance, w => w.Active)
				.AddBinding(vm => vm.CanChangeShowNomenclatureInstance, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
