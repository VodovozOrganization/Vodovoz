using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class FuelConsumptionReport : ViewBase<FuelConsumptionReportViewModel>
	{
		public FuelConsumptionReport(FuelConsumptionReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		void ConfigureDlg()
		{
			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			chkDetailed.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Detailed, w => w.Active)
				.InitializeFromSource();

			geographicGroup.UoW = ViewModel.UoW;
			geographicGroup.Label = "Часть города:";
			geographicGroup.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.GeoGroups, w => w.Items)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
