using System;
using QS.Views.GtkUI;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Domain.Store;
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
			comboWarehouse.SetRenderTextFunc<Warehouse>(x => x.Name);
			comboWarehouse.Binding.AddBinding(ViewModel, vm => vm.AvailableWarehouses, w => w.ItemsList).InitializeFromSource();
			comboWarehouse.Binding.AddBinding(ViewModel, vm => vm.Warehouse, w => w.SelectedItem).InitializeFromSource();
			comboWarehouse.Binding.AddBinding(ViewModel, vm => vm.CanChangeWarehouse, w => w.Sensitive).InitializeFromSource();

			checkShowArchive.Binding.AddBinding(ViewModel, vm => vm.ShowArchive, w => w.Active).InitializeFromSource();
			checkShowArchive.Binding.AddBinding(ViewModel, vm => vm.CanChangeShowArchive, w => w.Sensitive).InitializeFromSource();
		}
	}
}
