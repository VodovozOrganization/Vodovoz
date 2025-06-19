using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NomenclatureBalanceByStockFilterView : FilterViewBase<NomenclatureBalanceByStockFilterViewModel>
	{
		public NomenclatureBalanceByStockFilterView(NomenclatureBalanceByStockFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			comboWarehouse.SetRenderTextFunc<Warehouse>(x => x.Name);
			comboWarehouse.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.AvailableWarehouses, w => w.ItemsList)
				.AddBinding(vm => vm.Warehouse, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanChangeWarehouse, w => w.Sensitive).InitializeFromSource();

			entryNomenclature.Subject = ViewModel.Nomenclature;
			entryNomenclature.Binding.AddBinding(ViewModel, vm => vm.CanChangeNomenclature, w => w.Sensitive).InitializeFromSource();
		}
	}
}
