using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Sales;

namespace Vodovoz.ReportsParameters
{
	public partial class IncomeBalanceReport : ViewBase<IncomeBalanceReportViewModel>
	{
		public IncomeBalanceReport(IncomeBalanceReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumcomboboxReportType.ItemsEnum = ViewModel.IncomeReportTypeType;
			yenumcomboboxReportType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ReportType, w => w.SelectedItem)
				.InitializeFromSource();

			buttonCreateRepot.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
