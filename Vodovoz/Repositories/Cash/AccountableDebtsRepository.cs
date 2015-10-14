using System;
using QSOrmProject;
using Vodovoz.Domain;
using NHibernate.Criterion;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Repository.Cash
{
	public class AccountableDebtsRepository
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

	}
}

