using Gamma.Widgets.Additions;
using QS.Views;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.ReportsParameters.Logistic.CarOwnershipReport;
namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class CarOwnershipReportView : ViewBase<CarOwnershipReportViewModel>
	{
		public CarOwnershipReportView(CarOwnershipReportViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			yvboxMain.Sensitive = ViewModel.IsUserHasAccessToCarOwnershipReport;

			enumcheckCarTypeOfUse.EnumType = typeof(CarTypeOfUse);
			enumcheckCarTypeOfUse.Binding
				.AddBinding(ViewModel, vm => vm.SelectedCarTypesOfUse, w => w.SelectedValuesList, new EnumsListConverter<CarTypeOfUse>())
				.InitializeFromSource();

			enumcheckCarOwnType.EnumType = typeof(CarOwnType);
			enumcheckCarOwnType.Binding
				.AddBinding(ViewModel, vm => vm.SelectedCarOwnTypes, w => w.SelectedValuesList, new EnumsListConverter<CarOwnType>())
				.InitializeFromSource();

			yradiobuttonCarOwnTypeOnDate.Binding
				.AddBinding(ViewModel, vm => vm.IsOneDayReportSelected, w => w.Active)
				.InitializeFromSource();

			yradiobuttonCarOwnTypeOnPeriod.Binding
				.AddBinding(ViewModel, vm => vm.IsPeriodReportSelected, w => w.Active)
				.InitializeFromSource();

			datepickerDate.Binding
				.AddBinding(ViewModel, vm => vm.DateInOneDayReport, w => w.DateOrNull)
				.InitializeFromSource();
			datepickerDate.IsEditable = true;

			daterangepickerPeriod.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDateInPeriodReport, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDateInPeriodReport, w => w.EndDateOrNull)
				.InitializeFromSource();

			yhboxDateSettings.Binding
				.AddBinding(ViewModel, vm => vm.IsOneDayReportSelected, w => w.Sensitive)
				.InitializeFromSource();

			ytablePeriodSettings.Binding
				.AddBinding(ViewModel, vm => vm.IsPeriodReportSelected, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonCreateReport.Clicked += (s, e) => ViewModel.GenerateReportCommand.Execute();
		}
	}
}
