using QS.DomainModel.UoW;

namespace CustomerAppsApi.Library.V1.Repositories
{
	public interface ICachedBottlesDebtRepository
	{
		int GetCounterpartyBottlesDebt(IUnitOfWork uow, int counterpartyId, int counterpartyDebtCacheMinutes);
	}
}
