using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface ICategoryRepository
	{
		IncomeCategory DefaultIncomeCategory(IUnitOfWork uow);
		IncomeCategory RouteListClosingIncomeCategory(IUnitOfWork uow);
		ExpenseCategory RouteListClosingExpenseCategory(IUnitOfWork uow);
		ExpenseCategory FuelDocumentExpenseCategory(IUnitOfWork uow);
		ExpenseCategory EmployeeSalaryExpenseCategory(IUnitOfWork uow);
		IList<IncomeCategory> IncomeCategories(IUnitOfWork uow);
		IList<IncomeCategory> SelfDeliveryIncomeCategories(IUnitOfWork uow);
		IList<ExpenseCategory> ExpenseCategories(IUnitOfWork uow);
		IList<ExpenseCategory> ExpenseSelfDeliveryCategories(IUnitOfWork uow);
		QueryOver<ExpenseCategory> ExpenseCategoriesQuery();
		QueryOver<IncomeCategory> IncomeCategoriesQuery();
		int GetRouteListClosingExpenseCategoryId();
		int GetRouteListClosingIncomeCategoryId();
	}
}
