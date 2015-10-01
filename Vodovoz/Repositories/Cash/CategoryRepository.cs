using System;
using Vodovoz.Domain.Cash;
using QSOrmProject;
using System.Collections.Generic;

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
	}
}

