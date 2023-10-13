using System;
using Autofac;
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
		private DriverWarehouseEventName _eventName;
		private DriverWarehouseEventType? _selectedEventType;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Employee _driver;
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

		public IEntityEntryViewModel DriverWarehouseEventNameViewModel { get; private set; }
		public IEntityEntryViewModel DriverViewModel { get; private set; }
		public IEntityEntryViewModel CarViewModel { get; private set; }

		public DriverWarehouseEventName EventName
		{
			get => _eventName;
			set => UpdateFilterField(ref _eventName, value);
		}

		public DriverWarehouseEventType? SelectedEventType
		{
			get => _selectedEventType;
			set => UpdateFilterField(ref _selectedEventType, value);
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}
		
		public DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}
		
		public Employee Driver
		{
			get => _driver;
			set => SetField(ref _driver, value);
		}
		
		public Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}
		
		private void ConfigureEntryViewModels()
		{
			var builder = new CommonEEVMBuilderFactory<CompletedDriversWarehousesEventsJournalFilterViewModel>(
				_journalViewModel, this, UoW, _navigationManager, _scope);

			DriverWarehouseEventNameViewModel =
				builder.ForProperty(x => x.EventName)
					.UseViewModelJournalAndAutocompleter<DriversWarehousesEventsNamesJournalViewModel>()
					.UseViewModelDialog<DriverWarehouseEventNameViewModel>()
					.Finish();
			
			DriverViewModel =
				builder.ForProperty(x => x.Driver)
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
