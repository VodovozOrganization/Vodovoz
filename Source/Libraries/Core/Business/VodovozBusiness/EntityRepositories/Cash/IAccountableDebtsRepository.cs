using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface IAccountableDebtsRepository
	{
		decimal EmployeeDebt(IUnitOfWork uow, Employee accountable);
		decimal TotalEmployeeDebt(IUnitOfWork uow, Employee accountable);
		IList<Expense> UnclosedAdvance(IUnitOfWork uow, Employee accountable, ExpenseCategory category, int? organisationId);
	}
}
