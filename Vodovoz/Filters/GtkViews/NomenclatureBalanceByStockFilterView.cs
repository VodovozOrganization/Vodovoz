using QS.Views.GtkUI;
using Vodovoz.Domain.Store;
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
			//FIXME использовать журнал?
			comboWarehouse.SetRenderTextFunc<Warehouse>(x => x.Name);
			comboWarehouse.Binding.AddBinding(ViewModel, vm => vm.AvailableWarehouses, w => w.ItemsList).InitializeFromSource();
			comboWarehouse.Binding.AddBinding(ViewModel, vm => vm.Warehouse, w => w.SelectedItem).InitializeFromSource();
			comboWarehouse.Binding.AddBinding(ViewModel, vm => vm.CanChangeWarehouse, w => w.Sensitive).InitializeFromSource();
//FIXME возможно, стоит менять visible, а не sens, уточнить
			entryNomenclature.Subject = ViewModel.Nomenclature;
			entryNomenclature.Binding.AddBinding(ViewModel, vm => vm.CanChangeNomenclature, w => w.Sensitive).InitializeFromSource();
		}
	}
}
