using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderRegistryReport : ViewBase<OrderRegistryReportViewModel>
	{
		public OrderRegistryReport(OrderRegistryReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			ydatepicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.DateOrNull)
				.InitializeFromSource();

			geograficGroup.Label = "Часть города:";
			geograficGroup.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.UoW, w => w.UoW)
				.AddBinding(vm => vm.GeoGroups, w => w.Items)
				.InitializeFromSource();

			buttonCreateRepot.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
