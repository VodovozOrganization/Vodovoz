using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets.Users;
using static Vodovoz.ViewModels.Widgets.Users.WarehousesUserSelectionViewModel;

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
			ytreeviewWarehouses.CreateFluentColumnsConfig<WarehouseNode>()
				.AddColumn("Номер")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(n => n.WarehouseId)
				.AddColumn("Склад")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.WarehouseName)
					.WrapWidth(250).WrapMode(Pango.WrapMode.WordChar)
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
