using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Fuel;
namespace Vodovoz.ReportsParameters.Fuel
{
	public partial class FuelApiRequestReportView : ViewBase<FuelApiRequestReportViewModel>
	{
		public FuelApiRequestReportView(FuelApiRequestReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Initialize();
		}

		private void Initialize()
		{
			daterangepickerPeriod.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entityentryUser.ViewModel = ViewModel.UserViewModel;

			ybuttonCreate.Binding
				.AddBinding(ViewModel, vm => vm.CanCreateReport, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreate.BindCommand(ViewModel.CreateReportCommand);
		}
	}
}
