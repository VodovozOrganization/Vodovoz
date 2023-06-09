using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Cash
{
	public class AccountableDebtsRepository : IAccountableDebtsRepository
	{
		public decimal EmployeeDebt(IUnitOfWork uow, Employee accountable)
		{
			decimal received = uow.Session.QueryOver<Expense>()
				.Where(e => e.Employee == accountable && e.TypeOperation == ExpenseType.Advance)
				.Select (Projections.Sum<Expense> (o => o.Money)).SingleOrDefault<decimal> ();

			decimal returned = uow.Session.QueryOver<Income>()
				.Where(i => i.Employee == accountable && i.TypeOperation == IncomeType.Return)
				.Select (Projections.Sum<Income> (o => o.Money)).SingleOrDefault<decimal> ();

			decimal reported = uow.Session.QueryOver<AdvanceReport>()
				.Where(a => a.Accountable == accountable)
				.Select (Projections.Sum<AdvanceReport> (o => o.Money)).SingleOrDefault<decimal> ();
			
			return received - returned - reported;
		}

		public decimal TotalEmployeeDebt(IUnitOfWork uow, Employee accountable)
		{
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;

			var deliveredRouteListsTotalSum = uow.Session.QueryOver(() => routeListItemAlias)
				.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.Where(() => routeListAlias.Driver == accountable)
				.Where(() => routeListItemAlias.Status == RouteListItemStatus.Completed
							|| (routeListItemAlias.Status == RouteListItemStatus.EnRoute)
								&& (routeListAlias.Status == RouteListStatus.OnClosing || routeListAlias.Status == RouteListStatus.Closed))
				.Select(Projections.Sum(() => routeListItemAlias.TotalCash))
				.SingleOrDefault<decimal>();

			decimal received = uow.Session.QueryOver<Expense>()
				.Where(e => e.Employee == accountable && e.TypeOperation != ExpenseType.Salary)
				.Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			decimal returned = uow.Session.QueryOver<Income>()
				.Where(i => i.Employee == accountable)
				.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			decimal reported = uow.Session.QueryOver<AdvanceReport>()
				.Where(a => a.Accountable == accountable)
				.Select(Projections.Sum<AdvanceReport>(o => o.Money)).SingleOrDefault<decimal>();

			return received + deliveredRouteListsTotalSum - returned - reported;
		}

		public IList<Expense> UnclosedAdvance(IUnitOfWork uow, Employee accountable, ExpenseCategory category, int? organisationId)
		{
			var query = uow.Session.QueryOver<Expense>()
				.Where(e => e.Employee == accountable
				            && e.TypeOperation == ExpenseType.Advance
				            && e.AdvanceClosed == false);
				
			if(category != null)
				query.And (e => e.ExpenseCategory == category);
			
			if(organisationId != null)
				query.And(e => e.Organisation == null || e.Organisation.Id == organisationId);
			
			return query.List ();
		}
	}
}

