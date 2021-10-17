﻿using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Goods;
using QS.Widgets.GtkUI;
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
			var warehouseEntry = new EntityViewModelEntry();
			warehouseEntry.SetEntityAutocompleteSelectorFactory(ViewModel.WarehouseSelectorFactory);
			warehouseEntry.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Warehouse, w => w.Subject)
				.AddBinding(vm => vm.CanChangeWarehouse, w => w.Sensitive)
				.InitializeFromSource();

			warehouseEntry.Show();
			yhboxWarehouse.Add(warehouseEntry);

			checkShowArchive.Binding.AddBinding(ViewModel, vm => vm.ShowArchive, w => w.Active).InitializeFromSource();
			checkShowArchive.Binding.AddBinding(ViewModel, vm => vm.CanChangeShowArchive, w => w.Sensitive).InitializeFromSource();
		}
	}
}
