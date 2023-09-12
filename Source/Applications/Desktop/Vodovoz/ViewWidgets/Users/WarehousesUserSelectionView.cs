using QS.Views.GtkUI;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Widgets.Users;

namespace Vodovoz.ViewWidgets.Users
{
	public partial class WarehousesUserSelectionView : WidgetViewBase<WarehousesUserSelectionViewModel>
	{
		public WarehousesUserSelectionView(WarehousesUserSelectionViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ytreeviewWarehouses.CreateFluentColumnsConfig<Warehouse>()
				.AddColumn("Id склада")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n => n.Id)
					.XAlign(0.5f)
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeviewWarehouses.ItemsDataSource = ViewModel.ObservableWarehouses;

			ytreeviewWarehouses.Binding
				.AddBinding(ViewModel, vm => vm.SelectedWarehouse, w => w.SelectedRow)
				.InitializeFromSource();

			ybuttonDeleteWarehouseFromList.Binding
				.AddBinding(ViewModel, vm => vm.CanRemoveWarehouse, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonAddWarehouseToList.Clicked += (s, e) => ViewModel.AddWarehouseCommand?.Execute();
			ybuttonDeleteWarehouseFromList.Clicked += (s, e) => ViewModel.RemoveWarehouseCommand?.Execute();
		}
	}
}
