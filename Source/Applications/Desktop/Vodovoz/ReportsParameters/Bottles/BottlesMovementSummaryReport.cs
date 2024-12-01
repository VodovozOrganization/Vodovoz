using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Bottles;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottlesMovementSummaryReport : ViewBase<BottlesMovementSummaryReportViewModel>
	{
		public BottlesMovementSummaryReport(BottlesMovementSummaryReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ychkbtnShowDisposableTare.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ShowDisposableTare, w => w.Active)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
