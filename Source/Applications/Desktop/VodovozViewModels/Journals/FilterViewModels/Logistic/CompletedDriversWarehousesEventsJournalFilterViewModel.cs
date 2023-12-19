using System;
using Autofac;
using DateTimeHelpers;
using QS.Navigation;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class CompletedDriversWarehousesEventsJournalFilterViewModel :
		FilterViewModelBase<CompletedDriversWarehousesEventsJournalFilterViewModel>
	{
		private readonly DialogViewModelBase _journalViewModel;
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _scope;
		private int? _completedEventId;
		private decimal? _distanceFromScanning;
		private DriverWarehouseEvent _driverWarehouseEvent;
		private DriverWarehouseEventType? _selectedEventType;
		private DateTime? _startDate = DateTime.Today;
		private DateTime? _endDate = DateTime.Today;
		private Employee _employee;
		private Car _car;

		public CompletedDriversWarehousesEventsJournalFilterViewModel(
			DialogViewModelBase journalViewModel,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			Action<CompletedDriversWarehousesEventsJournalFilterViewModel> filterParameters = null)
		{
			_journalViewModel = journalViewModel ?? throw new ArgumentNullException(nameof(journalViewModel));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			ConfigureEntryViewModels();
			
			if(filterParameters != null)
			{
				SetAndRefilterAtOnce(filterParameters);
			}
		}

		public IEntityEntryViewModel DriverWarehouseEventViewModel { get; private set; }
		public IEntityEntryViewModel DriverViewModel { get; private set; }
		public IEntityEntryViewModel CarViewModel { get; private set; }

		public int? CompletedEventId
		{
			get => _completedEventId;
			set => SetField(ref _completedEventId, value);
		}

		public decimal? DistanceFromScanning
		{
			get => _distanceFromScanning;
			set => SetField(ref _distanceFromScanning, value);
		}

		public DriverWarehouseEvent DriverWarehouseEvent
		{
			get => _driverWarehouseEvent;
			set => UpdateFilterField(ref _driverWarehouseEvent, value);
		}

		public DriverWarehouseEventType? SelectedEventType
		{
			get => _selectedEventType;
			set => UpdateFilterField(ref _selectedEventType, value);
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}
		
		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}
		
		public Employee Employee
		{
			get => _employee;
			set => UpdateFilterField(ref _employee, value);
		}
		
		public Car Car
		{
			get => _car;
			set => UpdateFilterField(ref _car, value);
		}
		
		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<CompletedDriversWarehousesEventsJournalFilterViewModel>(
				_journalViewModel, this, UoW, _navigationManager, _scope);

			DriverWarehouseEventViewModel =
				builder.ForProperty(x => x.DriverWarehouseEvent)
					.UseViewModelJournalAndAutocompleter<DriversWarehousesEventsJournalViewModel>()
					.UseViewModelDialog<DriverWarehouseEventViewModel>()
					.Finish();
			
			DriverViewModel =
				builder.ForProperty(x => x.Employee)
					.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
					.UseViewModelDialog<EmployeeViewModel>()
					.Finish();
			
			CarViewModel =
				builder.ForProperty(x => x.Car)
					.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
					.UseViewModelDialog<CarViewModel>()
					.Finish();
		}
	}
}
