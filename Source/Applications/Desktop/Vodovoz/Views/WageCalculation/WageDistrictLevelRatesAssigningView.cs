using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.ViewModels.WageCalculation;
using static Vodovoz.ViewModels.ReportsParameters.Cash.DayOfSalaryGiveoutReportViewModel;
namespace Vodovoz.Views.WageCalculation
{
	public partial class WageDistrictLevelRatesAssigningView : TabViewBase<WageDistrictLevelRatesAssigningViewModel>
	{
		public WageDistrictLevelRatesAssigningView(WageDistrictLevelRatesAssigningViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}
		private void Configure()
		{
			yenumcomboboxEmployeeCategory.ItemsEnum = typeof(EmployeeCategory);
			yenumcomboboxEmployeeCategory.HiddenItems = new object[] { EmployeeCategory.office };
			yenumcomboboxEmployeeCategory.ShowSpecialStateAll = true;
			yenumcomboboxEmployeeCategory.Binding
				.AddBinding(ViewModel, vm => vm.Category, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			ytreeviewEmployees.ColumnsConfig = ColumnsConfigFactory.Create<EmployeeSelectableNode>()
				.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Имя").AddTextRenderer(d => d.FullName)
				.AddColumn("Выбрать").AddToggleRenderer(d => d.IsSelected)
				.Finish();
			ytreeviewEmployees.SetItemsSource(ViewModel.EmployeeNodes);

			speciallistcomboboxWageLevelRatesFilter.ItemsList = ViewModel.WageLevels;
			speciallistcomboboxWageLevelRatesFilter.ShowSpecialStateNot = true;
			speciallistcomboboxWageLevelRatesFilter.Binding
				.AddBinding(ViewModel, vm => vm.WageDistrictLevelRatesFilter, w => w.SelectedItem)
				.InitializeFromSource();

			ycheckbuttonExcludeSelectedLevelRate.Binding
				.AddBinding(ViewModel, vm => vm.IsExcludeSelectedInFilterWageDistrictLevelRates, w => w.Active)
				.InitializeFromSource();

			datepickerStartDate.IsEditable = true;
			datepickerStartDate.Binding
				.AddBinding(ViewModel, vm => vm.StartDate, w => w.DateOrNull)
				.InitializeFromSource();

			speciallistcomboboxDriverCarWageLivelRate.ItemsList = ViewModel.WageLevels;
			speciallistcomboboxDriverCarWageLivelRate.ShowSpecialStateNot = true;
			speciallistcomboboxDriverCarWageLivelRate.Binding
				.AddBinding(ViewModel, vm => vm.WageDistrictLevelRatesForDriverCars, w => w.SelectedItem)
				.InitializeFromSource();

			speciallistcomboboxCompanyCarWageLivelRate.ItemsList = ViewModel.WageLevels;
			speciallistcomboboxCompanyCarWageLivelRate.ShowSpecialStateNot = true;
			speciallistcomboboxCompanyCarWageLivelRate.Binding
				.AddBinding(ViewModel, vm => vm.WageDistrictLevelRatesForCompanyCars, w => w.SelectedItem)
				.InitializeFromSource();

			speciallistcomboboxRaskatCarWageLivelRate.ItemsList = ViewModel.WageLevels;
			speciallistcomboboxRaskatCarWageLivelRate.ShowSpecialStateNot = true;
			speciallistcomboboxRaskatCarWageLivelRate.Binding
				.AddBinding(ViewModel, vm => vm.WageDistrictLevelRatesForRaskatCars, w => w.SelectedItem)
				.InitializeFromSource();

			ybuttonSelectAll.BindCommand(ViewModel.SelectAllEmployeesCommand);
			ybuttonClearSelected.BindCommand(ViewModel.UnselectAllEmployeesCommand);
			ybuttonUpdateEmployeesWageLevelRates.BindCommand(ViewModel.UpdateWageDistrictLevelRatesCommand);
		}
	}
}
