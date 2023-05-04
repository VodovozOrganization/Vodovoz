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
			warehouseEntry.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeWarehouse, w => w.Sensitive)
				.InitializeFromSource();

			checkShowArchive.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowArchive, w => w.Active)
				.AddBinding(vm => vm.CanChangeShowArchive, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
