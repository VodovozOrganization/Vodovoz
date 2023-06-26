using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface IAccountableDebtsRepository
	{
		decimal EmployeeDebt(IUnitOfWork uow, Employee accountable);
		IList<Expense> GetUnclosedAdvances(IUnitOfWork uow, Employee accountable, ExpenseCategory category, int? organisationId);
		IEnumerable<Expense> GetUnclosedAdvances(IUnitOfWork unitOfWork, Employee accountableEmployee, int? expenseCategoryId, int? organisationId);
	}
}
