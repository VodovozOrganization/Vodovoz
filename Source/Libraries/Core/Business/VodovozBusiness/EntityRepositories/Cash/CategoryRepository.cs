using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Parameters;

namespace Vodovoz.EntityRepositories.Cash
{
	public class CategoryRepository : ICategoryRepository
	{
		private readonly IParametersProvider _parametersProvider;
		const string defaultIncomeCategory 			 = "default_income_category";
		const string routeListClosingIncomeCategory  = "routelist_income_category_id";
		const string routeListClosingExpenseCategory = "routelist_expense_category_id";
		const string fuelDocumentExpenseCategory 	 = "fuel_expense";
		const string employeeSalaryExpenseCategory   = "employee_salary"; 		// Параметр базы для статьи расхода для авансов.

		public CategoryRepository(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public IList<IncomeCategory> IncomeCategories(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<IncomeCategory> ().OrderBy (ic => ic.Name).Asc ().List ();
		}

		public IList<IncomeCategory> SelfDeliveryIncomeCategories(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<IncomeCategory>()
				.Where(x => x.IncomeDocumentType == IncomeInvoiceDocumentType.IncomeInvoiceSelfDelivery)
				.OrderBy(ic => ic.Name).Asc()
				.List();
		}

		public IList<ExpenseCategory> ExpenseCategories(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<ExpenseCategory>().OrderBy(ec => ec.Name).Asc().List();
		}

		public IList<ExpenseCategory> ExpenseSelfDeliveryCategories(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<ExpenseCategory>()
				.Where(x => x.ExpenseDocumentType == ExpenseInvoiceDocumentType.ExpenseInvoiceSelfDelivery)
				.OrderBy(ec => ec.Name).Asc()
				.List();
		}

		public QueryOver<ExpenseCategory> ExpenseCategoriesQuery()
		{
			return QueryOver.Of<ExpenseCategory> ().OrderBy (ec => ec.Name).Asc ();
		}

		public QueryOver<IncomeCategory> IncomeCategoriesQuery()
		{
			return QueryOver.Of<IncomeCategory> ().OrderBy (ic => ic.Name).Asc ();
		}

		public IncomeCategory DefaultIncomeCategory(IUnitOfWork uow)
		{
			if (_parametersProvider.ContainsParameter(defaultIncomeCategory)) {
				int id = -1;
				id = int.Parse(_parametersProvider.GetParameterValue(defaultIncomeCategory));
				if (id == -1)
				{
					return null;
				}

				return uow.Session.QueryOver<IncomeCategory> ()
					.Where (inc => inc.Id == id)
					.Take (1)
					.SingleOrDefault ();
			}
			return null;
		}

		public int GetRouteListClosingIncomeCategoryId()
			=> int.Parse(_parametersProvider.GetParameterValue(routeListClosingIncomeCategory));

		public IncomeCategory RouteListClosingIncomeCategory(IUnitOfWork uow)
		{
			if (_parametersProvider.ContainsParameter(routeListClosingIncomeCategory))
			{
				int id = -1;
				id = int.Parse(_parametersProvider.GetParameterValue(routeListClosingIncomeCategory));
				
				if (id == -1)
				{
					return null;
				}

				return uow.Session.QueryOver<IncomeCategory> ()
					.Where (inc => inc.Id == id)
					.Take (1)
					.SingleOrDefault ();
			}
			return null;
		}

		public int GetRouteListClosingExpenseCategoryId()
			=> int.Parse(_parametersProvider.GetParameterValue(routeListClosingExpenseCategory));

		public ExpenseCategory RouteListClosingExpenseCategory(IUnitOfWork uow)
		{
			if (_parametersProvider.ContainsParameter(routeListClosingExpenseCategory))
			{
				int id = -1;
				id = int.Parse(_parametersProvider.GetParameterValue(routeListClosingExpenseCategory));
				
				if (id == -1)
				{
					return null;
				}

				return uow.Session.QueryOver<ExpenseCategory> ()
					.Where (inc => inc.Id == id)
					.Take (1)
					.SingleOrDefault ();
			}
			return null;
		}

		public ExpenseCategory FuelDocumentExpenseCategory(IUnitOfWork uow)
		{
			if(_parametersProvider.ContainsParameter(fuelDocumentExpenseCategory))
			{
				int id = -1;
				id = int.Parse(_parametersProvider.GetParameterValue(fuelDocumentExpenseCategory));
				
				if (id == -1)
				{
					return null;
				}

				return uow.Session.QueryOver<ExpenseCategory> ()
					.Where (fExp => fExp.Id == id)
					.Take (1)
					.SingleOrDefault ();
			}
			return null;
		}

		public ExpenseCategory EmployeeSalaryExpenseCategory(IUnitOfWork uow)
		{
			if(_parametersProvider.ContainsParameter(employeeSalaryExpenseCategory))
			{
				int id = -1;
				id = int.Parse(_parametersProvider.GetParameterValue(employeeSalaryExpenseCategory));
				
				if(id == -1)
				{
					return null;
				}

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

