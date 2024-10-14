using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Bookkeeping;

namespace Vodovoz.ReportsParameters.Bookkeeping
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyCloseDeliveryReport : ViewBase<CounterpartyCloseDeliveryReportViewModel>
	{
		public CounterpartyCloseDeliveryReport(CounterpartyCloseDeliveryReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			datePeriodPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
