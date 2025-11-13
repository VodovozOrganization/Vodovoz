using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class NonClosedRLByPeriodReport : ViewBase<NonClosedRLByPeriodReportViewModel>
	{
		public NonClosedRLByPeriodReport(NonClosedRLByPeriodReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yspinbtnDelay.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Delay, w => w.ValueAsInt)
				.InitializeFromSource();

			ybtnCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
