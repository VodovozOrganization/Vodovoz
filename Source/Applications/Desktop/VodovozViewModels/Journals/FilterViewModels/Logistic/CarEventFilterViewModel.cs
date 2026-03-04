using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class CarEventFilterViewModel : FilterViewModelBase<CarEventFilterViewModel>
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly ICommonServices _commonServices;
		private readonly INavigationManager _navigationManager;
		private DateTime? _createEventDateFrom;
		private DateTime? _createEventDateEndTo;
		private DateTime? _startEventDateFrom;
		private DateTime? _startEventDateTo;
		private DateTime? _endEventDateFrom;
		private DateTime? _endEventDateTo;
		private Employee _author;
		private Car _car;
		private Employee _driver;
		private CarEventType _carEventType;
		private DialogViewModelBase _journal;

		public CarEventFilterViewModel(
			ILifetimeScope lifetimeScope,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ICarEventTypeJournalFactory carEventTypeJournalFactory,
			IEmployeeJournalFactory employeeJournalFactory)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			CarEventTypeSelectorFactory =
				(carEventTypeJournalFactory ?? throw new ArgumentNullException(nameof(carEventTypeJournalFactory)))
				.CreateCarEventTypeAutocompleteSelectorFactory();
			EmployeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();
			DriverSelectorFactory = employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
		}

		public IEntityAutocompleteSelectorFactory CarEventTypeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }
		public IEntityEntryViewModel CarEntryViewModel { get; private set; }

		public DialogViewModelBase Journal
		{
			get => _journal;
			set
			{
				if(_journal is null)
				{
					_journal = value;

					var carViewModelBuilder = new CommonEEVMBuilderFactory<CarEventFilterViewModel>(Journal, this, UoW, _navigationManager, _lifetimeScope);

					var viewModel = carViewModelBuilder
						.ForProperty(x => x.Car)
						.UseViewModelDialog<CarViewModel>()
						.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
							filter =>
							{
							})
						.Finish();

					viewModel.CanViewEntity = _commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

					CarEntryViewModel = viewModel;
				}
			}
		}

		public DateTime? CreateEventDateFrom
		{
			get => _createEventDateFrom;
			set => UpdateFilterField(ref _createEventDateFrom, value);
		}

		public DateTime? CreateEventDateTo
		{
			get => _createEventDateEndTo;
			set => UpdateFilterField(ref _createEventDateEndTo, value);
		}

		public DateTime? StartEventDateFrom
		{
			get => _startEventDateFrom;
			set => UpdateFilterField(ref _startEventDateFrom, value);
		}

		public DateTime? StartEventDateTo
		{
			get => _startEventDateTo;
			set => UpdateFilterField(ref _startEventDateTo, value);
		}

		public DateTime? EndEventDateFrom
		{
			get => _endEventDateFrom;
			set => UpdateFilterField(ref _endEventDateFrom, value);
		}

		public DateTime? EndEventDateTo
		{
			get => _endEventDateTo;
			set => UpdateFilterField(ref _endEventDateTo, value);
		}

		public Employee Author
		{
			get => _author;
			set => UpdateFilterField(ref _author, value);
		}

		public Car Car
		{
			get => (_car);
			set => UpdateFilterField(ref _car, value);
		}

		public Employee Driver
		{
			get => _driver;
			set => UpdateFilterField(ref _driver, value);
		}

		public CarEventType CarEventType
		{
			get => _carEventType;
			set => UpdateFilterField(ref _carEventType, value);
		}
		public List<int> ExcludeEventIds { get; } = new List<int>();
	}
}
