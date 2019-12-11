using System;
using System.ComponentModel.DataAnnotations;
using QS.Commands;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.Services;
using QS.Utilities;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Filters.ViewModels
{
	public class CallTaskFilterViewModel : FilterViewModelBase<CallTaskFilterViewModel>, IJournalFilter
	{
		public CallTaskFilterViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
			StartDate = DateTime.Today.AddDays(-14);
			EndDate = DateTime.Today.AddDays(14);
			CreateCommands();
		}

		private DateTime startDate;
		public DateTime StartDate {
			get => startDate;
			set => UpdateFilterField(ref startDate, value);
		}

		private DateTime endDate;
		public DateTime EndDate {
			get => endDate;
			set => UpdateFilterField(ref endDate, value);
		}

		private TaskFilterDateType dateType = TaskFilterDateType.DeadlinePeriod;
		public TaskFilterDateType DateType {
			get => dateType;
			set => UpdateFilterField(ref dateType, value);
		}

		private Employee employee;
		public Employee Employee {
			get => employee;
			set => UpdateFilterField(ref employee, value);
		}

		private DeliveryPointCategory deliveryPointCategory;
		public DeliveryPointCategory DeliveryPointCategory {
			get => deliveryPointCategory;
			set => UpdateFilterField(ref deliveryPointCategory, value);
		}

		private bool hideCompleted = true;
		public bool HideCompleted {
			get => hideCompleted;
			set => UpdateFilterField(ref hideCompleted, value);
		}

		private bool showOnlyWithoutEmployee = true;
		public bool ShowOnlyWithoutEmployee {
			get => showOnlyWithoutEmployee;
			set => UpdateFilterField(ref showOnlyWithoutEmployee, value);
		}

		private SortingParamType sortingParam = SortingParamType.Id;
		public SortingParamType SortingParam {
			get => sortingParam;
			set => UpdateFilterField(ref sortingParam, value);
		}

		private SortingDirectionType sortingDirection;
		public SortingDirectionType SortingDirection {
			get => sortingDirection;
			set => UpdateFilterField(ref sortingDirection, value);
		}

		public void SetDatePeriod(DateTime startPeriod, DateTime endPeriod)
		{
			StartDate = startPeriod;
			EndDate = endPeriod;
		}

		public void SetDatePeriod(int weekIndex)
		{
			DateHelper.GetWeekPeriod(out DateTime start_date, out DateTime end_date, weekIndex);
			SetDatePeriod(start_date, end_date);
		}

		void CreateCommands()
		{
			CreateChangeDateOnExpiredCommand();
			CreateChangeDateOnTodayCommand();
			CreateChangeDateOnTomorrowCommand();
			CreateChangeDateOnThisWeekCommand();
			CreateChangeDateOnNextWeekCommand();
		}

		#region Commands
		public DelegateCommand ChangeDateOnExpiredCommand { get; private set; }
		void CreateChangeDateOnExpiredCommand()
		{
			ChangeDateOnExpiredCommand = new DelegateCommand(
				() => {
					StartDate = DateTime.Now.AddDays(-15);
					EndDate = DateTime.Now.AddDays(-1);
				}, () => true
			);
		}

		public DelegateCommand ChangeDateOnTodayCommand { get; private set; }
		void CreateChangeDateOnTodayCommand()
		{
			ChangeDateOnTodayCommand = new DelegateCommand(
				() => {
					StartDate = DateTime.Now;
					EndDate = DateTime.Now;
				}, () => true
			);
		}

		public DelegateCommand ChangeDateOnTomorrowCommand { get; private set; }
		void CreateChangeDateOnTomorrowCommand()
		{
			ChangeDateOnTomorrowCommand = new DelegateCommand(
				() => {
					StartDate = DateTime.Now.AddDays(1);
					EndDate = DateTime.Now.AddDays(1);
				}, () => true
			);
		}

		public DelegateCommand ChangeDateOnThisWeekCommand { get; private set; }
		void CreateChangeDateOnThisWeekCommand()
		{
			ChangeDateOnThisWeekCommand = new DelegateCommand(
				() => {
					DateHelper.GetWeekPeriod(out DateTime start_date, out DateTime end_date, 0);
					StartDate = start_date;
					EndDate = end_date;
				}, () => true
			);
		}

		public DelegateCommand ChangeDateOnNextWeekCommand { get; private set; }
		void CreateChangeDateOnNextWeekCommand()
		{
			ChangeDateOnNextWeekCommand = new DelegateCommand(
				() => {
					DateHelper.GetWeekPeriod(out DateTime start_date, out DateTime end_date, 1);
					StartDate = start_date;
					EndDate = end_date;
				}, () => true
			);
		}
		#endregion
	}

	public enum TaskFilterDateType
	{
		[Display(Name = "Дата создания задачи")]
		CreationTime,
		[Display(Name = "Дата выполнения задачи")]
		CompleteTaskDate,
		[Display(Name = "Период выполнения задачи")]
		DeadlinePeriod
	}

	public enum SortingParamType
	{
		[Display(Name = "Клиент")]
		Client,
		[Display(Name = "Адрес")]
		DeliveryPoint,
		[Display(Name = "№")]
		Id,
		[Display(Name = "Долг по адресу")]
		DebtByAddress,
		[Display(Name = "Долг по клиенту")]
		DebtByClient,
		[Display(Name = "Создатель задачи")]
		Deadline,
		[Display(Name = "Ответственный")]
		AssignedEmployee,
		[Display(Name = "Статус")]
		Status,
		[Display(Name = "Срочность")]
		ImportanceDegree
	}

	public enum SortingDirectionType
	{
		[Display(Name = "От меньшего к большему")]
		FromSmallerToBigger,
		[Display(Name = "От большего к меньшему")]
		FromBiggerToSmaller
	}
}
