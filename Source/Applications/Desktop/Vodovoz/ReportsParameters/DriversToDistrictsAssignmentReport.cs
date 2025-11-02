using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.ReportsParameters
{
	public partial class DriversToDistrictsAssignmentReport : ViewBase<DriversToDistrictsAssignmentReportViewModel>
	{
		public DriversToDistrictsAssignmentReport(DriversToDistrictsAssignmentReportViewModel viewModel) : base(viewModel)
		{
			Build();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			onlyDifferentDistricts.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OnlyDifferentDistricts, w => w.Active)
				.InitializeFromSource();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
