using QS.DomainModel.UoW;

namespace CustomerAppsApi.Library.V2.Repositories
{
	public interface ICachedBottlesDebtRepository
	{
		int GetCounterpartyBottlesDebt(IUnitOfWork uow, int counterpartyId, int counterpartyDebtCacheMinutes);
	}
}
