using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Payments;

namespace Vodovoz.ReportsParameters.Payments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsFromAvangardReport : ViewBase<PaymentsFromAvangardReportViewModel>
	{

		public PaymentsFromAvangardReport(PaymentsFromAvangardReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		void Configure()
		{
			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.AddBinding(vm => vm.IsCustomPeriod, w => w.Sensitive)
				.InitializeFromSource();

			rbtnCustomPeriod.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsCustomPeriod, w => w.Active)
				.InitializeFromSource();

			rbtnLast3Days.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsLast3DaysPeriod, w => w.Active)
				.InitializeFromSource();

			rbtnYesterday.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsYesterdayPeriod, w => w.Active)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
