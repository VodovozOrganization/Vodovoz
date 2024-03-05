using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Logistics.Drivers;
using QS.ViewModels.Widgets;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Logistic
{
	public class CompletedDriversWarehousesEventsJournalFilterViewModel :
		FilterViewModelBase<CompletedDriversWarehousesEventsJournalFilterViewModel>
	{
		private readonly ILeftRightListViewModelFactory _leftRightListViewModelFactory;
		private readonly DialogViewModelBase _journalViewModel;
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _scope;
		private readonly SelectableParametersReportFilter _filter;
		private int? _completedEventId;
		private decimal? _distanceFromScanning;
		private DriverWarehouseEvent _driverWarehouseEvent;
		private DriverWarehouseEventType? _selectedEventType;
		private DateTime? _startDate = DateTime.Today;
		private DateTime? _endDate = DateTime.Today;
		private bool _orderByEventDateDesc = true;
		private Car _car;
		private ComparisonSings _distanceCriterion = ComparisonSings.LessOrEqual;

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
			_filter = new SelectableParametersReportFilter(UoW);

			ConfigureEntryViewModels();
			SetupSorting();
			ConfigureSelectableFilter();

			if(filterParameters != null)
			{
				SetAndRefilterAtOnce(filterParameters);
			}
		}

		public IEntityEntryViewModel DriverWarehouseEventViewModel { get; private set; }
		public IEntityEntryViewModel CarViewModel { get; private set; }
		public LeftRightListViewModel<GroupingNode> SortViewModel { get; private set; }
		public SelectableParameterReportFilterViewModel SelectableParameterReportFilterViewModel { get; private set; }

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
		
		public Car Car
		{
			get => _car;
			set => UpdateFilterField(ref _car, value);
		}
		
		public bool OrderByEventDateDesc
		{
			get => _orderByEventDateDesc;
			set => UpdateFilterField(ref _orderByEventDateDesc, value);
		}

		public ComparisonSings DistanceCriterion
		{
			get => _distanceCriterion;
			set => UpdateFilterField(ref _distanceCriterion, value);
		}

		public IEnumerable<int> GetSelectedDrivers()
		{
			var driversSet = SelectableParameterReportFilterViewModel.ReportFilter.ParameterSets.FirstOrDefault();

			return driversSet is null
				? Array.Empty<int>()
				: driversSet.GetIncludedParameters().Select(x => (int)x.Value).ToArray();
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

		private void ConfigureSelectableFilter()
		{
			_filter.CreateParameterSet(
				"Сотрудники",
				"employees",
				new ParametersFactory(UoW, (filters) =>
				{
					SelectableEntityParameter<Employee> resultAlias = null;
					var query = UoW.Session.QueryOver<Employee>();

					if(filters != null && filters.Any())
					{
						foreach(var f in filters)
						{
							query.Where(f());
						}
					}

					var employeeProjection = CustomProjections.Concat_WS(
						" ",
						Projections.Property<Employee>(x => x.LastName),
						Projections.Property<Employee>(x => x.Name),
						Projections.Property<Employee>(x => x.Patronymic)
					);

					query.SelectList(list => list
							.Select(x => x.Id).WithAlias(() => resultAlias.EntityId)
							.Select(employeeProjection).WithAlias(() => resultAlias.EntityTitle))
						.OrderBy(x => x.LastName).Asc
						.OrderBy(x => x.Name).Asc
						.OrderBy(x => x.Patronymic).Asc
						.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<Employee>>());
					
					return query.List<SelectableParameter>();
				})
			);

			SelectableParameterReportFilterViewModel = new SelectableParameterReportFilterViewModel(_filter);
		}
	}
}
