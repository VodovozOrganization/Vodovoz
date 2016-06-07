using System;
using QSOrmProject;
using NHibernate.Criterion;
using Vodovoz.Domain.Cash;

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

	}
}

