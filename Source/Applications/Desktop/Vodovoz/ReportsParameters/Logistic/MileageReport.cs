using Autofac;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Report;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.Widgets;
using QSReport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ReportsParameters.Logistic
{
	public partial class MileageReport : SingleUoWWidgetBase, IParametersWidget, INotifyPropertyChanged
	{
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ILifetimeScope _lifetimeScope;

		private ITdiTab _parentTab;
		private Car _car;

		public MileageReport(
			IReportInfoFactory reportInfoFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ILifetimeScope lifetimeScope)
		{
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			Build();
			Configure();
		}

		private void Configure()
		{
			var uowFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();
			UoW = uowFactory.CreateWithoutRoot();

			ConfigureEntries();

			ycheckbutton1.Toggled += (sender, args) =>
			{
				entityentryCar.Sensitive = !ycheckbutton1.Active;
				entityviewmodelentryEmployee.Sensitive = !ycheckbutton1.Active;
				Car = null;
				entityviewmodelentryEmployee.Subject = null;
			};

			validatedentryDifference.ValidationMode = ValidationType.Numeric;
		}

		private void ConfigureEntries()
		{
			entityviewmodelentryEmployee.SetEntityAutocompleteSelectorFactory(
				_employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory());
		}

		#region Properties
		public Car Car
		{
			get => _car;
			set
			{
				if(_car != value)
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
		#endregion Properties

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var navigationManager = _lifetimeScope.BeginLifetimeScope().Resolve<INavigationManager>();

			var viewModel = new LegacyEEVMBuilderFactory<MileageReport>(ParentTab, this, UoW, navigationManager, _lifetimeScope)
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

		public string Title => "Отчет по километражу";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{
				{ "start_date", dateperiodpicker.StartDateOrNull },
				{ "end_date", dateperiodpicker.EndDateOrNull },
				{ "our_cars_only", ycheckbutton1.Active },
				{ "any_status", checkAnyStatus.Active },
				{ "car_id", Car?.Id ?? 0 },
				{ "employee_id", (entityviewmodelentryEmployee.Subject as Employee)?.Id ?? 0 },
				{ "difference_km", validatedentryDifference.Text }
			};

			var reportInfo = _reportInfoFactory.Create("Logistic.MileageReport", Title, parameters);
			reportInfo.UseUserVariables = true;

			return reportInfo;
		}

		private void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}

		private void UpdateCanRun()
		{
			buttonCreateReport.Sensitive =
				(dateperiodpicker.EndDateOrNull != null && dateperiodpicker.StartDateOrNull != null);
		}

		private void OnDateperiodpickerPeriodChanged(object sender, EventArgs e)
		{
			UpdateCanRun();
		}
	}
}
