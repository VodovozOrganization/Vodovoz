using System;
using System.ComponentModel.DataAnnotations;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using QS.DomainModel.Entity;
using QS.Tools;
using QS.Utilities;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Filters
{
	public sealed class CallTaskFilter : PropertyChangedBase, IQueryFilter
	{
		private DateTime startDate;
		public DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		private DateTime endDate;
		public DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value, () => EndDate);
		}

		private TaskFilterDateType dateType = TaskFilterDateType.DeadlinePeriod;
		public TaskFilterDateType DateType {
			get => dateType;
			set => SetField(ref dateType, value, () => DateType);
		}

		private Employee employee;
		public Employee Employee {
			get => employee;
			set => SetField(ref employee, value, () => Employee);
		}

		private bool hideCompleted = true;
		public bool HideCompleted {
			get => hideCompleted;
			set => SetField(ref hideCompleted, value, () => HideCompleted);
		}

		private bool showOnlyWihoutEmployee = true;
		public bool ShowOnlyWihoutEmployee {
			get => showOnlyWihoutEmployee;
			set => SetField(ref showOnlyWihoutEmployee, value, () => ShowOnlyWihoutEmployee);
		}

		public void SetDatePeriod(DateTime startPeriod, DateTime endPeriod)
		{
			StartDate = startPeriod;
			EndDate = endPeriod;
		}

		public void SetDatePeriod(int weekIndex)
		{
			DateHelper.GetWeekPeriod(out DateTime start_date, out DateTime end_date , weekIndex);
			SetDatePeriod(start_date, end_date);
		}

		public CallTaskFilter()
		{
			StartDate = DateTime.Today.AddDays(-14);
			EndDate = DateTime.Today.AddDays(14);
		}

		public ICriterion GetFilter()
		{
			ICriterion result;

			IProjection dateProjection = GetDateProjection();

			result = Restrictions.And(Restrictions.Ge(dateProjection , StartDate.Date), Restrictions.Le(dateProjection , EndDate.Date));

			if(Employee != null) 
				result = Restrictions.And(result, Restrictions.Where<CallTask>(x => x.AssignedEmployee == Employee));
			else if(ShowOnlyWihoutEmployee)
				result = Restrictions.And(result, Restrictions.Where<CallTask>(x => x.AssignedEmployee == null));

			if(HideCompleted)
				result = Restrictions.And(result, Restrictions.Where<CallTask>(x => !x.IsTaskComplete));

			return result;
		}

		private IProjection GetDateProjection()
		{
			PropertyProjection dateProperty;
			switch(DateType) {
				case TaskFilterDateType.CreationTime:
					dateProperty = Projections.Property<CallTask>(x => x.CreationDate);
					break;
				case TaskFilterDateType.CompleteTaskDate:
					dateProperty = Projections.Property<CallTask>(x => x.CompleteDate);
					break;
				default:
					dateProperty = Projections.Property<CallTask>(x => x.EndActivePeriod);
					break;
			}

			IProjection dateCriterion = Projections.SqlFunction(
				   new SQLFunctionTemplate(
					   NHibernateUtil.Date,
					   "Date(?1)"
					  ),
				   NHibernateUtil.Date,
				   dateProperty
			);
			return dateCriterion;
		}

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
}
