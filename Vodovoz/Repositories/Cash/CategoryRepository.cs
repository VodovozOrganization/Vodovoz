using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Cash;
using QSSupportLib;

namespace Vodovoz.Repository.Cash
{
	public class CategoryRepository
	{
		const string defaultIncomeCategory = "default_income_category";

		public static IList<IncomeCategory> IncomeCategories (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<IncomeCategory> ().List ();
		}

		public static IList<ExpenseCategory> ExpenseCategories (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<ExpenseCategory> ().List ();
		}

		public static QueryOver<ExpenseCategory> ExpenseCategoriesQuery ()
		{
			return QueryOver.Of<ExpenseCategory> ();
		}

		public static QueryOver<IncomeCategory> IncomeCategoriesQuery ()
		{
			return QueryOver.Of<IncomeCategory> ();
		}

		public static IncomeCategory DefaultIncomeCategory (IUnitOfWork uow)
		{
			if (MainSupport.BaseParameters.All.ContainsKey (defaultIncomeCategory)) {
				int id = -1;
				id = int.Parse (MainSupport.BaseParameters.All [defaultIncomeCategory]);
				if (id == -1)
					return null;
				return uow.Session.QueryOver<IncomeCategory> ()
					.Where (inc => inc.Id == id)
					.Take (1)
					.SingleOrDefault ();
			}
			return null;
		}
	}
}

