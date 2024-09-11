using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Widgets.Goods;

namespace Vodovoz.Views.Goods
{
	[ToolboxItem(true)]
	public partial class NomenclatureMinimumBalanceByWarehouseView : WidgetViewBase<NomenclatureMinimumBalanceByWarehouseViewModel>
	{
		public NomenclatureMinimumBalanceByWarehouseView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			yvboxMain.Binding.AddBinding(ViewModel, vm => vm.UnlockMainBox, w => w.Sensitive).InitializeFromSource();
			yhboxEdit.Binding.AddBinding(ViewModel, vm => vm.ShowEditBox, w => w.Visible).InitializeFromSource();

			entityentryWarehouse.ViewModel = ViewModel.WarehouseEntryViewModel;
			yspinbuttonAmount.Binding.AddBinding(ViewModel, vm => vm.CurrentMinimumBalance, w => w.ValueAsInt).InitializeFromSource();

			ytreeMinimumBalances.ColumnsConfig = FluentColumnsConfig<NomenclatureMinimumBalanceByWarehouse>.Create()
				.AddColumn("Номер").AddNumericRenderer(x => x.Id).XAlign(0.5f)
				.AddColumn("Наименование").AddTextRenderer(x => x.Warehouse.Name).XAlign(0.5f)
				.AddColumn("Количество").AddNumericRenderer(x => x.MinimumBalance).XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeMinimumBalances.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Balances, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedNomenclatureMinimumBalanceByWarehouse, w => w.SelectedRow)
				.InitializeFromSource();

			ybuttonAdd.BindCommand(ViewModel.AddCommand);
			ybuttonCancel.BindCommand(ViewModel.CancelCommand);
			ybuttonSave.BindCommand(ViewModel.SaveCommand);
			ybuttonDelete.BindCommand(ViewModel.DeleteCommand);
			ybuttonEdit.BindCommand(ViewModel.EditCommand);
		}
	}
}
