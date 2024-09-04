using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Payments;

namespace Vodovoz.ReportsParameters.Payments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsFromBankClientFinDepartmentReport : ViewBase<PaymentsFromBankClientFinDepartmentReportViewModel>
	{
		public PaymentsFromBankClientFinDepartmentReport(PaymentsFromBankClientFinDepartmentReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			daterangepicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			btnCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
