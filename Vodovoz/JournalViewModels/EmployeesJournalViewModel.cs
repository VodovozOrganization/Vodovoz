using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.Config;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalNodes;

namespace Vodovoz.JournalViewModels
{
	public class EmployeesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Employee, EmployeeDlg, EmployeeJournalNode, EmployeeFilterViewModel>
	{
		public EmployeesJournalViewModel(
			EmployeeFilterViewModel filterViewModel,
			ICommonServices commonServices
		) : base(
			filterViewModel,
			commonServices
		)
		{
			SetOrder(
				new Dictionary<Func<EmployeeJournalNode, object>, bool> {
					{x => x.EmpLastName , false},
					{x => x.EmpFirstName , false},
					{x => x.EmpMiddleName , false}
				}
			);
			UpdateOnChanges(typeof(Employee));
		}

		protected override Func<IQueryOver<Employee>> ItemsSourceQueryFunction => () => {
			EmployeeJournalNode resultAlias = null;
			Employee employeeAlias = null;

			var query = UoW.Session.QueryOver(() => employeeAlias);

			if(!FilterViewModel.ShowFired)
				query.Where(e => !e.IsFired);

			if(FilterViewModel.Category != null)
				query.Where(e => e.Category == FilterViewModel.Category);

			if(FilterViewModel.RestrictWageType.HasValue) {
				var subquery = QueryOver.Of<WageParameter>()
										.Where(p => p.WageParameterType == FilterViewModel.RestrictWageType.Value)
										.Where(p => p.EndDate == null || p.EndDate >= DateTime.Today)
										.Select(p => p.Employee.Id)
				;
				query.WithSubquery.WhereProperty(e => e.Id).In(subquery);
			}

			var result = query
				.SelectList(list => list
				   .Select(() => employeeAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => employeeAlias.IsFired).WithAlias(() => resultAlias.IsFired)
				   .Select(() => employeeAlias.Name).WithAlias(() => resultAlias.EmpFirstName)
				   .Select(() => employeeAlias.LastName).WithAlias(() => resultAlias.EmpLastName)
				   .Select(() => employeeAlias.Patronymic).WithAlias(() => resultAlias.EmpMiddleName)
				   .Select(() => employeeAlias.Category).WithAlias(() => resultAlias.EmpCatEnum)
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
