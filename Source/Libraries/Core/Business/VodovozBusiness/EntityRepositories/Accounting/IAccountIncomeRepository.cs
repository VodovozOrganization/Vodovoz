using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Accounting
{
	public interface IAccountIncomeRepository
	{
		bool AccountIncomeExists(IUnitOfWork uow, int year, int number, string counterpartyInn, string accountNumber);
	}
}