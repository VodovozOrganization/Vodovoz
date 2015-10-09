using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Repository.Cash
{
	public class CategoryRepository
	{
		public static IList<IncomeCategory> IncomeCategories (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<IncomeCategory> ().List ();
		}

		public static IList<ExpenseCategory> ExpenseCategories (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<ExpenseCategory> ().List ();
		}

		public static QueryOver<ExpenseCategory> ExpenseCategoriesQuery()
		{
			return QueryOver.Of<ExpenseCategory> ();
		}

		public static QueryOver<IncomeCategory> IncomeCategoriesQuery()
		{
			return QueryOver.Of<IncomeCategory> ();
		}

	}
}

