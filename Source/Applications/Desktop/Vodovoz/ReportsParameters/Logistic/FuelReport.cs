using Autofac;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Report;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QSProjectsLib;
using QSReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Settings.Car;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets.Cars.CarModelSelection;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Reports
{
	public partial class FuelReport : SingleUoWWidgetBase, IParametersWidget, INotifyPropertyChanged
	{
		private ITdiTab _parentTab;
		private CarModelSelectionFilterViewModel _carModelSelectionFilterViewModel;
		private readonly IReportInfoFactory _reportFactory;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly INavigationManager _navigationManager;
		private Car _car;

		public FuelReport(
			IUnitOfWorkFactory unitOfWorkFactory,
			IReportInfoFactory reportFactory,
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager,
			IEmployeeJournalFactory employeeJournalFactory)
		{
			_reportFactory = reportFactory ?? throw new ArgumentNullException(nameof(reportFactory));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			Build();
			
			UoW = unitOfWorkFactory.CreateWithoutRoot();
			
			evmeDriver.SetEntityAutocompleteSelectorFactory(
				employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory(true));
			
			evmeAuthor.SetEntityAutocompleteSelectorFactory(
				employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory(true));
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
			buttonCreateReport.Clicked += OnButtonCreateReportClicked;

			ConfigureCarModelSelectionFilter();

			yvboxCarModel.Visible = false;
			var carModelSelectionFilterView = new CarModelSelectionFilterView(_carModelSelectionFilterViewModel);
			yhboxCarModelContainer.Add(carModelSelectionFilterView);
			carModelSelectionFilterView.Show();

			radioDriver.Visible = false;
			radioCar.Active = true;
		}

		public Car Car 
		{
			get => _car;
			set
			{
				if (_car != value)
				{
					_car = value;

					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Car)));
				}
			}
		}

		public ITdiTab ParentTab
		{
			get => _parentTab;
			set
			{
				_parentTab = value;

				if(entityentryCar.ViewModel == null)
				{
					entityentryCar.ViewModel = BuildCarEntryViewModel();
				}
			}
		}

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var viewModel = new LegacyEEVMBuilderFactory<FuelReport>(ParentTab, this, UoW, _navigationManager, _lifetimeScope)
			.ForProperty(x => x.Car)
			.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
				filter =>
				{
				})
			.UseViewModelDialog<CarViewModel>()
			.Finish();

			viewModel.CanViewEntity = ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
		}

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public event PropertyChangedEventHandler PropertyChanged;

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
				CarTypeOfUse.Minivan,
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
				parameters.Add("car_id", Car?.Id);
				parameters.Add("driver_id", -1);
			}

			string reportName = "Logistic.FuelReport";

			if(radioSumm.Active) {
				parameters.Add("author", (evmeAuthor.Subject as Employee)?.Id ?? -1);
				parameters.Add("include_car_models", _carModelSelectionFilterViewModel.IncludedCarModelNodesCount > 0 ? _carModelSelectionFilterViewModel.IncludedCarModelIds : new int[] { 0 });
				parameters.Add("exclude_car_models", _carModelSelectionFilterViewModel.ExcludedCarModelNodesCount > 0 ? _carModelSelectionFilterViewModel.ExcludedCarModelIds : new int[] { 0 });

				reportName = yCheckButtonDatailedSummary.Active ? "Logistic.FuelReportSummaryDetailed" : "Logistic.FuelReportSummaryBasic";
			}

			var reportInfo = _reportFactory.Create(reportName, Title, parameters);
			reportInfo.UseUserVariables = true;

			return reportInfo;
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			string errorString = string.Empty;

			if(radioDriver.Active && (dateperiodpicker.StartDateOrNull == null || evmeDriver.Subject == null)) {
				errorString += "Не заполнена дата или не выбран водитель\n";
			}

			if(radioCar.Active && (dateperiodpicker.StartDateOrNull == null | Car == null))
			{
				errorString += "Не заполнена дата или автомобиль\n";
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

			Car = null;
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

			Car = null;
			evmeDriver.Subject = null;

			yvboxCarModel.Visible = true;
		}

	}
}
