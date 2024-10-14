using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.Reports
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashierCommentsReport : ViewBase<CashierCommentsReportViewModel>
	{
		public CashierCommentsReport(CashierCommentsReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}

