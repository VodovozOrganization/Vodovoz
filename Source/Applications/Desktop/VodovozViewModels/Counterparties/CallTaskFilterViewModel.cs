using QS.Commands;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using QS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.Counterparties
{
	public partial class CallTaskFilterViewModel : FilterViewModelBase<CallTaskFilterViewModel>
	{
		private TaskFilterDateType _dateType = TaskFilterDateType.DeadlinePeriod;
		private DeliveryPointCategory _deliveryPointCategory;
		private Employee _employee;
		private DateTime _endDate;
		private bool _hideCompleted = true;
		private bool _showOnlyWithoutEmployee = true;
		private SortingDirectionType _sortingDirection;
		private SortingParamType _sortingParam = SortingParamType.Id;
		private DateTime _startDate;
		private GeoGroup _geographicGroup;
		private readonly IEmployeeJournalFactory _employeJournalFactory;

		public CallTaskFilterViewModel(
			IEmployeeJournalFactory employeJournalFactory,
			IDeliveryPointRepository deliveryPointRepository)
		{
			_employeJournalFactory = employeJournalFactory ?? throw new ArgumentNullException(nameof(employeJournalFactory));
			EmployeeAutocompleteSelectorFactory = _employeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			ActiveDeliveryPointCategories =
				deliveryPointRepository?.GetActiveDeliveryPointCategories(UoW)
				?? throw new ArgumentNullException(nameof(deliveryPointRepository));
			StartDate = DateTime.Today.AddDays(-14);
			EndDate = DateTime.Today.AddDays(14);
			GeographicGroups = UoW.Session.QueryOver<GeoGroup>().List();
			CreateCommands();
		}

		public IEntityAutocompleteSelectorFactory EmployeeAutocompleteSelectorFactory { get; }
		public IOrderedEnumerable<DeliveryPointCategory> ActiveDeliveryPointCategories { get; }

		public DateTime StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public DateTime EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public TaskFilterDateType DateType
		{
			get => _dateType;
			set => UpdateFilterField(ref _dateType, value);
		}

		public Employee Employee
		{
			get => _employee;
			set => UpdateFilterField(ref _employee, value);
		}

		public DeliveryPointCategory DeliveryPointCategory
		{
			get => _deliveryPointCategory;
			set => UpdateFilterField(ref _deliveryPointCategory, value);
		}

		public bool HideCompleted
		{
			get => _hideCompleted;
			set => UpdateFilterField(ref _hideCompleted, value);
		}

		public bool ShowOnlyWithoutEmployee
		{
			get => _showOnlyWithoutEmployee;
			set => UpdateFilterField(ref _showOnlyWithoutEmployee, value);
		}

		public SortingParamType SortingParam
		{
			get => _sortingParam;
			set => UpdateFilterField(ref _sortingParam, value);
		}

		public SortingDirectionType SortingDirection
		{
			get => _sortingDirection;
			set => UpdateFilterField(ref _sortingDirection, value);
		}

		public GeoGroup GeographicGroup
		{
			get => _geographicGroup;
			set => UpdateFilterField(ref _geographicGroup, value);
		}

		public IList<GeoGroup> GeographicGroups { get; }

		private void SetDatePeriod(DateTime startPeriod, DateTime endPeriod)
		{
			StartDate = startPeriod;
			EndDate = endPeriod;
		}

		public void SetDatePeriod(int weekIndex)
		{
			DateHelper.GetWeekPeriod(out DateTime start_date, out DateTime end_date, weekIndex);
			SetDatePeriod(start_date, end_date);
		}

		private void CreateCommands()
		{
			CreateChangeDateOnExpiredCommand();
			CreateChangeDateOnTodayCommand();
			CreateChangeDateOnTomorrowCommand();
			CreateChangeDateOnThisWeekCommand();
			CreateChangeDateOnNextWeekCommand();
		}

		#region Commands

		public DelegateCommand ChangeDateOnExpiredCommand { get; private set; }

		private void CreateChangeDateOnExpiredCommand()
		{
			ChangeDateOnExpiredCommand = new DelegateCommand(
				() =>
				{
					StartDate = DateTime.Now.AddDays(-15);
					EndDate = DateTime.Now.AddDays(-1);
				}, () => true
			);
		}

		public DelegateCommand ChangeDateOnTodayCommand { get; private set; }

		private void CreateChangeDateOnTodayCommand()
		{
			ChangeDateOnTodayCommand = new DelegateCommand(
				() =>
				{
					StartDate = DateTime.Now;
					EndDate = DateTime.Now;
				}, () => true
			);
		}

		public DelegateCommand ChangeDateOnTomorrowCommand { get; private set; }

		private void CreateChangeDateOnTomorrowCommand()
		{
			ChangeDateOnTomorrowCommand = new DelegateCommand(
				() =>
				{
					StartDate = DateTime.Now.AddDays(1);
					EndDate = DateTime.Now.AddDays(1);
				}, () => true
			);
		}

		public DelegateCommand ChangeDateOnThisWeekCommand { get; private set; }

		private void CreateChangeDateOnThisWeekCommand()
		{
			ChangeDateOnThisWeekCommand = new DelegateCommand(
				() =>
				{
					DateHelper.GetWeekPeriod(out DateTime start_date, out DateTime end_date, 0);
					StartDate = start_date;
					EndDate = end_date;
				}, () => true
			);
		}

		public DelegateCommand ChangeDateOnNextWeekCommand { get; private set; }

		private void CreateChangeDateOnNextWeekCommand()
		{
			ChangeDateOnNextWeekCommand = new DelegateCommand(
				() =>
				{
					DateHelper.GetWeekPeriod(out DateTime start_date, out DateTime end_date, 1);
					StartDate = start_date;
					EndDate = end_date;
				}, () => true
			);
		}

		#endregion
	}
}
