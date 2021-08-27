using System;
using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Domain.Store;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.JournalViewModels;
using QS.Widgets.GtkUI;
using Gamma.Widgets;
namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureStockFilterView : FilterViewBase<NomenclatureStockFilterViewModel>
	{
		public NomenclatureStockFilterView(NomenclatureStockFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			if(ViewModel.UserHasOnlyAccessToWarehouseAndComplaints)
			{
				var warehouseEntry = new EntityViewModelEntry();
				warehouseEntry.SetEntityAutocompleteSelectorFactory(ViewModel.WarehouseSelectorFactory);
				warehouseEntry.Binding.AddSource(ViewModel)
					.AddBinding(vm => vm.Warehouse, w => w.Subject)
					.AddBinding(vm => vm.CanChangeWarehouse, w => w.Sensitive)
					.InitializeFromSource();

				warehouseEntry.Show();
				yhboxWarehouse.Add(warehouseEntry);
			}
			else
			{
				var warehouseCombo = new ySpecComboBox();
				warehouseCombo.SetRenderTextFunc<Warehouse>(x => x.Name);
				warehouseCombo.Binding.AddSource(ViewModel)
					.AddBinding(vm => vm.AvailableWarehouses, w => w.ItemsList)
					.AddBinding(vm => vm.Warehouse, w => w.SelectedItem)
					.AddBinding(ViewModel, vm => vm.CanChangeWarehouse, w => w.Sensitive)
					.InitializeFromSource();

				warehouseCombo.Show();
				yhboxWarehouse.Add(warehouseCombo);
			}

			checkShowArchive.Binding.AddBinding(ViewModel, vm => vm.ShowArchive, w => w.Active).InitializeFromSource();
			checkShowArchive.Binding.AddBinding(ViewModel, vm => vm.CanChangeShowArchive, w => w.Sensitive).InitializeFromSource();
		}
	}
}
