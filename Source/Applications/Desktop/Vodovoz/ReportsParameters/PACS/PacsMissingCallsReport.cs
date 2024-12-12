using QS.Views;
using Vodovoz.Presentation.ViewModels.Pacs.Reports;

namespace Vodovoz.ReportsParameters.PACS
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PacsMissingCallsReport : ViewBase<PacsMissingCallsReportViewModel>
	{
		public PacsMissingCallsReport(PacsMissingCallsReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Confgigure();
		}

		private void Confgigure()
		{
			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DateFrom, w => w.StartDate)
				.AddBinding(vm => vm.DateTo, w => w.EndDate)
				.InitializeFromSource();
			
			buttonCreateRepot.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
