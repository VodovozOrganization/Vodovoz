using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;

namespace CustomerAppsApi.Library.Repositories
{
	public interface ICachedBottlesDebtRepository
	{
		Task<int> GetCounterpartyBottlesDebt(IUnitOfWork uow, int counterpartyId, CancellationToken cancellationToken = default);
	}
}
