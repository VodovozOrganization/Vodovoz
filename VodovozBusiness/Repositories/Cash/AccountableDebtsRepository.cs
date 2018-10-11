using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Repository.Cash
{
	public static class AccountableDebtsRepository
	{

		public static decimal EmloyeeDebt (IUnitOfWork uow, Employee accountable)
		{
			decimal recived = uow.Session.QueryOver<Expense>()
				.Where(e => e.Employee == accountable && e.TypeOperation == ExpenseType.Advance)
				.Select (Projections.Sum<Expense> (o => o.Money)).SingleOrDefault<decimal> ();

			decimal returned = uow.Session.QueryOver<Income>()
				.Where(i => i.Employee == accountable && i.TypeOperation == IncomeType.Return)
				.Select (Projections.Sum<Income> (o => o.Money)).SingleOrDefault<decimal> ();

			decimal reported = uow.Session.QueryOver<AdvanceReport>()
				.Where(a => a.Accountable == accountable)
				.Select (Projections.Sum<AdvanceReport> (o => o.Money)).SingleOrDefault<decimal> ();
			
			return recived - returned - reported;
		}

		public static IList<Expense> UnclosedAdvance(IUnitOfWork uow, Employee accountable, ExpenseCategory category)
		{
			var query = uow.Session.QueryOver<Expense> ()
				.Where (e => e.Employee == accountable && e.TypeOperation == ExpenseType.Advance && e.AdvanceClosed == false);
			if(category != null)
				query.And (e => e.ExpenseCategory == category);
			return query.List ();
		}
	}
}

