using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

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
			Domain.Orders.Order orderAlias = null;
			OrderItem orderItemAlias = null;
			OrderDepositItem orderDepositItemAlias = null;

			var orderTotalSumSubQuery = QueryOver.Of(() => orderItemAlias)
				.Where(() => orderAlias.Id == orderItemAlias.Order.Id)
				.Select(Projections.Sum(() => orderItemAlias.Price * orderItemAlias.ActualCount - orderItemAlias.DiscountMoney));

			var orderDepositSumSubQuery = QueryOver.Of(() => orderDepositItemAlias)
				.Where(() => orderAlias.Id == orderDepositItemAlias.Order.Id)
				.Select(Projections.Sum(() => orderDepositItemAlias.Deposit));

			var routeListItemCashProjection = Projections.Conditional(
				Restrictions.Gt(Projections.Property(() => routeListItemAlias.TotalCash), 0),
				Projections.Property(() => routeListItemAlias.TotalCash),
				Projections.SubQuery(orderTotalSumSubQuery)
			);

			var routeListItemTotalCashProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1,0) - IFNULL(?2,0) + IFNULL(?3,0) + IFNULL(?4,0) + IFNULL(?5,0)"),
				NHibernateUtil.Decimal,
				routeListItemCashProjection,
				Projections.SubQuery(orderDepositSumSubQuery),
				Projections.Property(() => routeListItemAlias.OldBottleDepositsCollected),
				Projections.Property(() => routeListItemAlias.OldEquipmentDepositsCollected),
				Projections.Property(() => routeListItemAlias.ExtraCash)
			);

			var deliveredRouteListsTotalSum = uow.Session.QueryOver(() => routeListItemAlias)
				.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.JoinAlias(() => routeListItemAlias.Order, () => orderAlias)
				.Where(() => routeListAlias.Driver == accountable)
				.Where(() => routeListItemAlias.Status == RouteListItemStatus.Completed
							|| (routeListItemAlias.Status == RouteListItemStatus.EnRoute)
								&& (routeListAlias.Status == RouteListStatus.OnClosing || routeListAlias.Status == RouteListStatus.Closed))
				.Where(() => orderAlias.PaymentType == Domain.Client.PaymentType.Cash)
				.Select(Projections.Sum(routeListItemTotalCashProjection))
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

			var debtSum = received + deliveredRouteListsTotalSum - returned - reported;

			return debtSum;
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

