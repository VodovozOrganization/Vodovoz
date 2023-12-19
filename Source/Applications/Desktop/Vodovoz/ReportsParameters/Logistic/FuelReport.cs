using Autofac;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QSProjectsLib;
using QSReport;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Settings.Car;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Widgets.Cars.CarModelSelection;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Reports
{
	public partial class FuelReport : SingleUoWWidgetBase, IParametersWidget
	{
		private CarModelSelectionFilterViewModel _carModelSelectionFilterViewModel;

		public FuelReport(
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager)
		{
			if(lifetimeScope is null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			var filterDriver = new EmployeeFilterViewModel();
			filterDriver.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			var driverFactory = new EmployeeJournalFactory(navigationManager, filterDriver);
			evmeDriver.SetEntityAutocompleteSelectorFactory(driverFactory.CreateEmployeeAutocompleteSelectorFactory());
			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(new CarJournalFactory(Startup.MainWin.NavigationManager).CreateCarAutocompleteSelectorFactory(lifetimeScope));
			entityviewmodelentryCar.CompletionPopupSetWidth(false);

			var officeFilter = new EmployeeFilterViewModel();
			officeFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking
			);
			var officeFactory = new EmployeeJournalFactory(navigationManager, officeFilter);
			evmeAuthor.SetEntityAutocompleteSelectorFactory(officeFactory.CreateEmployeeAutocompleteSelectorFactory());
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
			buttonCreateReport.Clicked += OnButtonCreateReportClicked;

			ConfigureCarModelSelectionFilter();

			yvboxCarModel.Visible = false;
			var carModelSelectionFilterView = new CarModelSelectionFilterView(_carModelSelectionFilterViewModel);
			yhboxCarModelContainer.Add(carModelSelectionFilterView);
			carModelSelectionFilterView.Show();
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по выдаче топлива";

		#endregion

		private void ConfigureCarModelSelectionFilter()
		{
			_carModelSelectionFilterViewModel = new CarModelSelectionFilterViewModel(UoW, Startup.AppDIContainer.Resolve<ICarSettings>());
			UpdateCarModelsList();
		}

		private void UpdateCarModelsList()
		{
			var carTypesOfUse = new List<CarTypeOfUse>
			{
				CarTypeOfUse.GAZelle,
				CarTypeOfUse.Largus,
				CarTypeOfUse.Truck
			};

			_carModelSelectionFilterViewModel.SelectedCarTypesOfUse = carTypesOfUse;
		}

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>();

			parameters.Add("start_date", dateperiodpicker.StartDateOrNull);
			parameters.Add("end_date", dateperiodpicker.EndDate.AddDays(1).AddTicks(-1));

			if(radioDriver.Active) {
				parameters.Add("car_id", -1);
				parameters.Add("driver_id", (evmeDriver.Subject as Employee)?.Id);
				parameters.Add("include_car_models", new int[] { 0 });
				parameters.Add("exclude_car_models", new int[] { 0 });

			}

			if(radioCar.Active) {
				parameters.Add("car_id", (entityviewmodelentryCar.Subject as Car)?.Id);
				parameters.Add("driver_id", -1);
				parameters.Add("include_car_models", new int[] { 0 });
				parameters.Add("exclude_car_models", new int[] { 0 });
			}

			if(radioSumm.Active) {
				parameters.Add("author", (evmeAuthor.Subject as Employee)?.Id ?? -1);
				parameters.Add("include_car_models", _carModelSelectionFilterViewModel.IncludedCarModelNodesCount > 0 ? _carModelSelectionFilterViewModel.IncludedCarModelIds : new int[] { 0 });
				parameters.Add("exclude_car_models", _carModelSelectionFilterViewModel.ExcludedCarModelNodesCount > 0 ? _carModelSelectionFilterViewModel.ExcludedCarModelIds : new int[] { 0 });

				return new ReportInfo {
					Identifier = yCheckButtonDatailedSummary.Active?"Logistic.FuelReportSummaryDetailed":"Logistic.FuelReportSummaryBasic",
					UseUserVariables = true,
					Parameters = parameters
				};
			}
			 
			return new ReportInfo {
				Identifier = "Logistic.FuelReport",
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			string errorString = string.Empty;

			if(radioDriver.Active && (dateperiodpicker.StartDateOrNull == null || evmeDriver.Subject == null)) {
				errorString += "Не заполнена дата\n Не заполнен водитель\n";
			}

			if(radioCar.Active && (dateperiodpicker.StartDateOrNull == null | entityviewmodelentryCar.Subject == null)) {
				errorString += "Не заполнена дата\n Не заполнен автомобиль\n";
			}
				
			if(radioSumm.Active && dateperiodpicker.StartDateOrNull == null)
				errorString += "Не заполнена дата\n";
			if(!string.IsNullOrWhiteSpace(errorString)) {
				MessageDialogWorks.RunErrorDialog(errorString);
				return;
			}
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		protected void OnRadioDriverToggled(object sender, EventArgs e)
		{
			hboxDriver.Visible = true;
			hboxCar.Visible = false;
			hboxAuthor.Visible = false;

			yCheckButtonDatailedSummary.Hide();

			entityviewmodelentryCar.Subject = null;
			evmeAuthor.Subject = null;

			yvboxCarModel.Visible = false;
			_carModelSelectionFilterViewModel.ClearAllIncludesCommand?.Execute();
			_carModelSelectionFilterViewModel.ClearAllExcludesCommand?.Execute();
			_carModelSelectionFilterViewModel.ClearSearchStringCommand?.Execute();
		}

		protected void OnRadioCarToggled(object sender, EventArgs e)
		{
			hboxDriver.Visible = false;
			hboxCar.Visible = true;
			hboxAuthor.Visible = false;

			yCheckButtonDatailedSummary.Hide();

			evmeAuthor.Subject = null;
			evmeDriver.Subject = null;

			yvboxCarModel.Visible = false;
			_carModelSelectionFilterViewModel.ClearAllIncludesCommand?.Execute();
			_carModelSelectionFilterViewModel.ClearAllExcludesCommand?.Execute();
			_carModelSelectionFilterViewModel.ClearSearchStringCommand?.Execute();
		}

		protected void OnRadioSummToggled(object sender, EventArgs e)
		{
			hboxDriver.Visible = false;
			hboxCar.Visible = false;
			hboxAuthor.Visible = true;

			yCheckButtonDatailedSummary.Show();

			entityviewmodelentryCar.Subject = null;
			evmeDriver.Subject = null;

			yvboxCarModel.Visible = true;
		}

	}
}
