using System;
using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Domain.Store;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.JournalViewModels;
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
			warehouseEntry.SetEntityAutocompleteSelectorFactory(ViewModel.WarehouseSelectorFactory);
			warehouseEntry.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Warehouse, w => w.Subject)
				.AddBinding(vm => vm.CanChangeWarehouse, w => w.Sensitive)
				.InitializeFromSource();

			checkShowArchive.Binding.AddBinding(ViewModel, vm => vm.ShowArchive, w => w.Active).InitializeFromSource();
			checkShowArchive.Binding.AddBinding(ViewModel, vm => vm.CanChangeShowArchive, w => w.Sensitive).InitializeFromSource();
		}
	}
}
