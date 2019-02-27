using System;
using QS.DomainModel.UoW;
using NHibernate.Criterion;
using Vodovoz.Domain.Cash;
using System.Collections.Generic;

namespace Vodovoz.Repository.Cash
{
	public class CashRepository
	{

		public static decimal GetIncomePaidSumForOrder(IUnitOfWork uow, int orderId, int? excludedIncomeDoc = null)
		{
			var query = uow.Session.QueryOver<Income>().Where(x => x.Order.Id == orderId);
			if(excludedIncomeDoc != null) {
				query.Where(x => x.Id != excludedIncomeDoc);
			}
			return query.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();
		}

		public static decimal GetExpenseReturnSumForOrder(IUnitOfWork uow, int orderId, int? excludedExpenseDoc = null)
		{
			var query = uow.Session.QueryOver<Expense>().Where(x => x.Order.Id == orderId);
			if(excludedExpenseDoc != null) {
				query.Where(x => x.Id != excludedExpenseDoc);
			}
			return query.Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();
		}

		public static decimal CurrentCash (IUnitOfWork uow)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
				.Select (Projections.Sum<Expense> (o => o.Money)).SingleOrDefault<decimal> ();

			decimal income = uow.Session.QueryOver<Income>()
				.Select (Projections.Sum<Income> (o => o.Money)).SingleOrDefault<decimal> ();

			return income - expense;
		}

		public static decimal CurrentCashForSubdivision(IUnitOfWork uow, Subdivision subdivision)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
				.Where(x => x.RelatedToSubdivision == subdivision)
				.Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			decimal income = uow.Session.QueryOver<Income>()
				.Where(x => x.RelatedToSubdivision == subdivision)
				.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			return income - expense;
		}

		public static Income GetIncomeByRouteList(IUnitOfWork uow, int routeListId)
		{
			return uow.Session.QueryOver<Income>()
				.Where(inc => inc.RouteListClosing.Id == routeListId)
				.Where(inc => inc.TypeOperation == IncomeType.DriverReport)
				.Take(1).SingleOrDefault();
		}

		public static Expense GetExpenseByRouteListId(IUnitOfWork uow, int routeListId)
		{
			return uow.Session.QueryOver<Expense>()
				.Where(exp => exp.RouteListClosing.Id == routeListId)
				.Where(exp => exp.TypeOperation == ExpenseType.Expense)
				.Take(1).SingleOrDefault();
		}

		public static decimal CurrentRouteListCash(IUnitOfWork uow, int routeListId)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
			                     .Where(exp => exp.RouteListClosing.Id == routeListId)
			                     .Where(exp => exp.TypeOperation == ExpenseType.Expense)
								 .Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			decimal income = uow.Session.QueryOver<Income>()
			                    .Where(exp => exp.RouteListClosing.Id == routeListId)
			                    .Where(exp => exp.TypeOperation == IncomeType.DriverReport)
								.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			return income - expense;
		}
	}
}

