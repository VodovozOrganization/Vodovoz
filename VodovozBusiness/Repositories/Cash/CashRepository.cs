using System;
using QS.DomainModel.UoW;
using NHibernate.Criterion;
using Vodovoz.Domain.Cash;
using System.Collections.Generic;

namespace Vodovoz.Repository.Cash
{
	public class CashRepository
	{

		public static decimal CurrentCash (IUnitOfWork uow)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
				.Select (Projections.Sum<Expense> (o => o.Money)).SingleOrDefault<decimal> ();

			decimal income = uow.Session.QueryOver<Income>()
				.Select (Projections.Sum<Income> (o => o.Money)).SingleOrDefault<decimal> ();

			return income - expense;
		}

		public static Income GetIncomeByRouteList(IUnitOfWork uow, int routeListId)
		{
			return uow.Session.QueryOver<Income>()
				.Where(inc => inc.RouteListClosing.Id == routeListId)
				.Where(inc => inc.TypeOperation == IncomeType.DriverReport)		// Выбирать только приход от водителя. Правка от 07.06.2017@Дима
				.Take(1).SingleOrDefault();
		}

		public static Expense GetExpenseByRouteListId(IUnitOfWork uow, int routeListId)
		{
			return uow.Session.QueryOver<Expense>()
				.Where(exp => exp.RouteListClosing.Id == routeListId)
				.Where(exp => exp.TypeOperation == ExpenseType.Expense)			// Выбирать только прочий расход. Правка от 07.06.2017@Дима
				.Take(1).SingleOrDefault();
		}

		public static decimal CurrentRouteListCash(IUnitOfWork uow, int routeListId)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
			                     .Where(exp => exp.RouteListClosing.Id == routeListId)
			                     .Where(exp => exp.TypeOperation == ExpenseType.Expense)            // Выбирать только прочий расход. Правка от 07.06.2017@Дима
								 .Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			decimal income = uow.Session.QueryOver<Income>()
			                    .Where(exp => exp.RouteListClosing.Id == routeListId)
			                    .Where(exp => exp.TypeOperation == IncomeType.DriverReport)     // Выбирать только приход от водителя. Правка от 07.06.2017@Дима
								.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			return income - expense;
		}
	}
}

