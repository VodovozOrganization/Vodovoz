using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace CustomerAppsApi.Library.Repositories
{
	public interface ICachedBottlesDebtRepository
	{
		int GetCounterpartyBottlesDebt(IUnitOfWork uow, int counterpartyId, int counterpartyDebtCacheMinutes);
	}
}
