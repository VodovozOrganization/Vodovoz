using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Parameters;

namespace Vodovoz.Repository.Cash
{
	public class CategoryRepository
	{
		const string defaultIncomeCategory 			 = "default_income_category";
		const string routeListClosingIncomeCategory  = "routelist_income_category_id";
		const string routeListClosingExpenseCategory = "routelist_expense_category_id";
		const string fuelDocumentExpenseCategory 	 = "fuel_expense";
		const string employeeSalaryExpenseCategory   = "employee_salary"; 		// Параметр базы для статьи расхода для авансов.

		public static IList<IncomeCategory> IncomeCategories (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<IncomeCategory> ().OrderBy (ic => ic.Name).Asc ().List ();
		}

		public static IList<IncomeCategory> SelfDeliveryIncomeCategories(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<IncomeCategory>()
				.Where(x => x.IncomeDocumentType == IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery)
				.OrderBy(ic => ic.Name).Asc()
				.List();
		}

		public static IList<ExpenseCategory> ExpenseCategories (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<ExpenseCategory> ().OrderBy (ec => ec.Name).Asc ().List ();
		}

		public static IList<ExpenseCategory> ExpenseSelfDeliveryCategories(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<ExpenseCategory>()
				.Where(x => x.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery)
				.OrderBy(ec => ec.Name).Asc()
				.List();
		}

		public static QueryOver<ExpenseCategory> ExpenseCategoriesQuery ()
		{
			return QueryOver.Of<ExpenseCategory> ().OrderBy (ec => ec.Name).Asc ();
		}

		public static QueryOver<IncomeCategory> IncomeCategoriesQuery ()
		{
			return QueryOver.Of<IncomeCategory> ().OrderBy (ic => ic.Name).Asc ();
		}

		public static IncomeCategory DefaultIncomeCategory (IUnitOfWork uow)
		{
			if (ParametersProvider.Instance.ContainsParameter(defaultIncomeCategory)) {
				int id = -1;
				id = int.Parse (ParametersProvider.Instance.GetParameterValue(defaultIncomeCategory));
				if (id == -1)
					return null;
				return uow.Session.QueryOver<IncomeCategory> ()
					.Where (inc => inc.Id == id)
					.Take (1)
					.SingleOrDefault ();
			}
			return null;
		}

		public static IncomeCategory RouteListClosingIncomeCategory(IUnitOfWork uow)
		{
			if (ParametersProvider.Instance.ContainsParameter(routeListClosingIncomeCategory))
			{
				int id = -1;
				id = int.Parse (ParametersProvider.Instance.GetParameterValue(routeListClosingIncomeCategory));
				if (id == -1)
					return null;
				return uow.Session.QueryOver<IncomeCategory> ()
					.Where (inc => inc.Id == id)
					.Take (1)
					.SingleOrDefault ();
			}
			return null;
		}

		public static ExpenseCategory RouteListClosingExpenseCategory(IUnitOfWork uow)
		{
			if (ParametersProvider.Instance.ContainsParameter(routeListClosingExpenseCategory))
			{
				int id = -1;
				id = int.Parse (ParametersProvider.Instance.GetParameterValue(routeListClosingExpenseCategory));
				if (id == -1)
					return null;
				return uow.Session.QueryOver<ExpenseCategory> ()
					.Where (inc => inc.Id == id)
					.Take (1)
					.SingleOrDefault ();
			}
			return null;
		}

		internal static Func<IUnitOfWork, ExpenseCategory> FuelDocumentExpenseCategoryTestGap;
		public static ExpenseCategory FuelDocumentExpenseCategory(IUnitOfWork uow)
		{
			if(FuelDocumentExpenseCategoryTestGap != null) {
				return FuelDocumentExpenseCategoryTestGap(uow);
			}

			if(ParametersProvider.Instance.ContainsParameter(fuelDocumentExpenseCategory))
			{
				int id = -1;
				id = int.Parse (ParametersProvider.Instance.GetParameterValue(fuelDocumentExpenseCategory));
				if (id == -1)
					return null;
				return uow.Session.QueryOver<ExpenseCategory> ()
					.Where (fExp => fExp.Id == id)
					.Take (1)
					.SingleOrDefault ();
			}
			return null;
		}

		public static ExpenseCategory EmployeeSalaryExpenseCategory(IUnitOfWork uow)
		{
			if(ParametersProvider.Instance.ContainsParameter(employeeSalaryExpenseCategory)) {
				int id = -1;
				id = int.Parse(ParametersProvider.Instance.GetParameterValue(employeeSalaryExpenseCategory));
				if(id == -1)
					return null;
				return uow.Session.QueryOver<ExpenseCategory>()
					.Where(fExp => fExp.Id == id)
					.Take(1)
					.SingleOrDefault();
			}
			return uow.Session.QueryOver<ExpenseCategory>()
					.Where(fExp => fExp.Id == 1)
					.Take(1)
					.SingleOrDefault();
		}
	}
}

