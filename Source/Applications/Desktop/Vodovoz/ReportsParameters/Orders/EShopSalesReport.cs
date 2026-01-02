using Gamma.Widgets.Additions;
using QS.Views;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
    public partial class EShopSalesReport : ViewBase<EShopSalesReportViewModel>
    {
		public EShopSalesReport(EShopSalesReportViewModel viewModel) : base(viewModel)
        {
            this.Build();
            Configure();
		}

        private void Configure()
        {
			datePeriodPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ycomboboxEShopId.SetRenderTextFunc<OnlineStore>(x => x.Name);
			ycomboboxEShopId.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OnlineStores, w => w.ItemsList)
				.AddBinding(vm => vm.OnlineStore, w => w.SelectedItem)
				.InitializeFromSource();

			enumchecklistOrderStatus.EnumType = ViewModel.OrderStatusType;
			enumchecklistOrderStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderStatuses, w => w.SelectedValuesList, new EnumsListConverter<OrderStatus>())
				.InitializeFromSource();
			enumchecklistOrderStatus.SelectAll();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);
		}
    }
}
