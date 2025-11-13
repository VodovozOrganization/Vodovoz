using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrdersByCreationDateReport : ViewBase<OrdersByCreationDateReportViewModel>
	{
		public OrdersByCreationDateReport(OrdersByCreationDateReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			pkrDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.DateOrNull)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
