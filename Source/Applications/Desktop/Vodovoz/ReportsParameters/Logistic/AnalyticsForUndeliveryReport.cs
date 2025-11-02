using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.ReportsParameters.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AnalyticsForUndeliveryReport : ViewBase<AnalyticsForUndeliveryReportViewModel>
	{
		public AnalyticsForUndeliveryReport(AnalyticsForUndeliveryReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
