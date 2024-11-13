using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Bottles;

namespace Vodovoz.ReportsParameters.Bottles
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class BottlesMovementReport : ViewBase<BottlesMovementReportViewModel>
	{
		public BottlesMovementReport(BottlesMovementReportViewModel viewModel) : base(viewModel)
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
