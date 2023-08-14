using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Cash
{
	public interface IExpenseCashOrganisationDistributor
	{
		void DistributeCashForExpense(IUnitOfWork uow, Expense expense, bool isSalary = false);
		void UpdateRecords(IUnitOfWork uow, ExpenseCashDistributionDocument document, Expense expense, Employee editor);
	}
}