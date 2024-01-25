using System;
using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Widgets;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class CompletedDriversWarehousesEventsJournalFilterViewModel :
		FilterViewModelBase<CompletedDriversWarehousesEventsJournalFilterViewModel>
	{
		private readonly ILeftRightListViewModelFactory _leftRightListViewModelFactory;
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
			ILeftRightListViewModelFactory leftRightListViewModelFactory,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			Action<CompletedDriversWarehousesEventsJournalFilterViewModel> filterParameters = null)
		{
			_journalViewModel = journalViewModel ?? throw new ArgumentNullException(nameof(journalViewModel));
			_leftRightListViewModelFactory =
				leftRightListViewModelFactory ?? throw new ArgumentNullException(nameof(leftRightListViewModelFactory));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			ConfigureEntryViewModels();
			SetupSorting();
			
			if(filterParameters != null)
			{
				SetAndRefilterAtOnce(filterParameters);
			}
		}

		public IEntityEntryViewModel DriverWarehouseEventViewModel { get; private set; }
		public IEntityEntryViewModel CarViewModel { get; private set; }
		public LeftRightListViewModel<GroupingNode> SortViewModel { get; private set; }

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
			
			CarViewModel =
				builder.ForProperty(x => x.Car)
					.UseViewModelJournalAndAutocompleter<CarJournalViewModel>()
					.UseViewModelDialog<CarViewModel>()
					.Finish();
		}
		
		private void SetupSorting()
		{
			SortViewModel = _leftRightListViewModelFactory.CreateCompletedDriverEventsSortingConstructor();
		}
	}
}
