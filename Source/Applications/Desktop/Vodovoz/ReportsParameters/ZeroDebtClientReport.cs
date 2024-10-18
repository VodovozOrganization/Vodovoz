using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Client;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ZeroDebtClientReport : ViewBase<ZeroDebtClientReportViewModel>
	{
		public ZeroDebtClientReport(ZeroDebtClientReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			button1.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
