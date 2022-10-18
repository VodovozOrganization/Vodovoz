using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Accounting
{
	public interface IAccountExpenseRepository
	{
		bool AccountExpenseExists(IUnitOfWork uow, int year, int number, string accountNumber);
	}
}