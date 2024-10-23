using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Logistics;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class ScheduleOnLinePerShiftReport : ViewBase<ScheduleOnLinePerShiftReportViewModel>
	{
		public ScheduleOnLinePerShiftReport(ScheduleOnLinePerShiftReportViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			geographicGroup.Label = "Часть города:";
			geographicGroup.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.UoW, w => w.UoW)
				.AddBinding(vm => vm.GeoGroups, w => w.Items)
				.InitializeFromSource();

			enumcheckCarTypeOfUse.EnumType = ViewModel.CarTypeOfUseType;
			enumcheckCarTypeOfUse.AddEnumToHideList(ViewModel.HiddenCarTypeOfUse);
			enumcheckCarTypeOfUse.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CarTypeOfUseList, w => w.SelectedValuesList)
				.InitializeFromSource();
			enumcheckCarTypeOfUse.SelectAll();

			enumcheckCarOwnType.EnumType = ViewModel.CarOwnTypeType;
			enumcheckCarOwnType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CarOwnTypeList, w => w.SelectedValuesList)
				.InitializeFromSource();
			enumcheckCarOwnType.SelectAll();

			buttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
