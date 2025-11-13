using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class DriverRoutesListRegisterReport : ViewBase<DriverRoutesListRegisterReportViewModel>
	{
		public DriverRoutesListRegisterReport(DriverRoutesListRegisterReportViewModel viewModel) : base(viewModel)
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

			chkMasters.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsDriverMaster, w => w.Active)
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
