using Autofac;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Report;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
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

namespace Vodovoz.ReportsParameters
{
	public partial class WayBillReport : SingleUoWWidgetBase, IParametersWidget, INotifyPropertyChanged
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;

		private ITdiTab _parentTab;
		private Car _car;

		public WayBillReport(
			ILifetimeScope lifetimeScope,
			IReportInfoFactory reportInfoFactory,
			IUnitOfWorkFactory uowFactory,
			IEmployeeJournalFactory employeeJournalFactory)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));

			Build();
			UoW = uowFactory.CreateWithoutRoot();
			Configure();
		}

		private void Configure()
		{
			datepicker.Date = DateTime.Today;
			timeHourEntry.Text = DateTime.Now.Hour.ToString("00.##");
			timeMinuteEntry.Text = DateTime.Now.Minute.ToString("00.##");

			entryDriver.SetEntityAutocompleteSelectorFactory(
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

			var viewModel = new LegacyEEVMBuilderFactory<WayBillReport>(ParentTab, this, UoW, navigationManager, _lifetimeScope)
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

		public string Title => "Путевой лист";

		#endregion

		private ReportInfo GetReportInfo()
		{
			var parameters = new Dictionary<string, object>
			{ 
				{ "date", datepicker.Date },
				{ "driver_id", (entryDriver?.Subject as Employee)?.Id ?? -1 },
				{ "car_id", Car?.Id ?? -1 },
				{ "time", timeHourEntry.Text + ":" + timeMinuteEntry.Text },
				{ "need_date", !datepicker.IsEmpty }
			};

			var reportInfo = _reportInfoFactory.Create("Logistic.WayBillReport", Title, parameters);
			return reportInfo;
		}

		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), true));
		}
	}
}
