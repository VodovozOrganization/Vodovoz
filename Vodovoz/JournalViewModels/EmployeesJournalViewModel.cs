using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels
{
	public class EmployeesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Employee, EmployeeDlg, EmployeeJournalNode, EmployeeFilterViewModel>
	{
		public EmployeesJournalViewModel(
			EmployeeFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices
		) : base(
			filterViewModel,
			unitOfWorkFactory,
			commonServices
		)
		{
			this.TabName = "Журнал сотрудников";
			UpdateOnChanges(typeof(Employee));
		}

		protected override Func<IUnitOfWork, IQueryOver<Employee>> ItemsSourceQueryFunction => (uow) => {
			EmployeeJournalNode resultAlias = null;
			Employee employeeAlias = null;
			DriverWorkSchedule drvWorkScheduleAlias = null;
			DeliveryDaySchedule dlvDayScheduleAlias = null;
			DeliveryShift shiftAlias = null;

			var query = uow.Session.QueryOver(() => employeeAlias);

			if(FilterViewModel?.Status != null)
				query.Where(e => e.Status == FilterViewModel.Status);

			if(FilterViewModel?.DrvStartTime != null && FilterViewModel.DrvEndTime != null && FilterViewModel.WeekDay != null) {
				query.Left.JoinAlias(() => employeeAlias.WorkDays, () => drvWorkScheduleAlias)
					 .Left.JoinAlias(() => drvWorkScheduleAlias.DaySchedule, () => dlvDayScheduleAlias)
					 .Left.JoinAlias(() => dlvDayScheduleAlias.Shifts, () => shiftAlias)
					 .Where(() => (int)drvWorkScheduleAlias.WeekDay == (int)FilterViewModel.WeekDay.Value.DayOfWeek
								   && shiftAlias.StartTime >= FilterViewModel.DrvStartTime
								   && shiftAlias.StartTime <= FilterViewModel.DrvEndTime);
			}

			if(FilterViewModel?.Category != null)
				query.Where(e => e.Category == FilterViewModel.Category);

			if(FilterViewModel?.RestrictWageParameterItemType != null) {
				WageParameterItem wageParameterItemAlias = null;
				var subquery = QueryOver.Of<EmployeeWageParameter>()
					.Left.JoinAlias(x => x.WageParameterItem, () => wageParameterItemAlias)
					.Where(() => wageParameterItemAlias.WageParameterItemType == FilterViewModel.RestrictWageParameterItemType.Value)
					.Where(p => p.EndDate == null || p.EndDate >= DateTime.Today)
					.Select(p => p.Employee.Id)
				;
				query.WithSubquery.WhereProperty(e => e.Id).In(subquery);
			}

			query.Where(GetSearchCriterion(
				() => employeeAlias.Name,
				() => employeeAlias.LastName,
				() => employeeAlias.Patronymic
			));

			var result = query
				.SelectList(list => list
				   .Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => employeeAlias.Status).WithAlias(() => resultAlias.Status)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmpFirstName)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmpLastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmpMiddleName)
				   .Select(() => employeeAlias.Category).WithAlias(() => resultAlias.EmpCatEnum)
				   .SelectGroup(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
				)
				.OrderBy(x => x.LastName).Asc
				.OrderBy(x => x.Name).Asc
				.OrderBy(x => x.Patronymic).Asc
				.TransformUsing(Transformers.AliasToBean<EmployeeJournalNode>())
				;
			return result;
		};

		protected override Func<EmployeeDlg> CreateDialogFunction => () => new EmployeeDlg();

		protected override Func<EmployeeJournalNode, EmployeeDlg> OpenDialogFunction => n => new EmployeeDlg(n.Id);
	}
}
