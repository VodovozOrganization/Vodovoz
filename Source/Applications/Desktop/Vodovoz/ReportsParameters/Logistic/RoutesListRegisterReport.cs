using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.Reports.Logistic
{
	public partial class RoutesListRegisterReport : ViewBase<RoutesListRegisterReportViewModel>
	{
		public RoutesListRegisterReport(RoutesListRegisterReportViewModel viewModel) : base(viewModel)
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

			geographicGroup.Label = "Часть города:";
			geographicGroup.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.UoW, w => w.UoW)
				.AddBinding(vm => vm.GeoGroups, w => w.Items)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
