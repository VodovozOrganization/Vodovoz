using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Cash
{
	public interface IAdvanceCashOrganisationDistributor
	{
		void DistributeCashForExpenseAdvance(IUnitOfWork uow, Expense expense, AdvanceReport advanceReport);
		void DistributeCashForIncomeAdvance(IUnitOfWork uow, Income income, AdvanceReport advanceReport);
	}
}