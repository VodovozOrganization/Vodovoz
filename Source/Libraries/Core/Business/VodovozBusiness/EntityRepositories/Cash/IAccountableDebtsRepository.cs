using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface IAccountableDebtsRepository
	{
		decimal EmployeeDebt(IUnitOfWork uow, Employee accountable);
		IEnumerable<Expense> GetUnclosedAdvances(IUnitOfWork unitOfWork, Employee accountableEmployee, int? expenseCategoryId, int? organisationId);
	}
}
