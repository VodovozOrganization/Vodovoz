using QS.DomainModel.UoW;
using System.Threading.Tasks;
using Vodovoz.Domain.Documents;

namespace Vodovoz.EntityRepositories.Store
{
	public interface ICarLoadDocumentRepository
	{
		decimal LoadedTerminalAmount(IUnitOfWork uow, int routelistId, int terminalId);
		Task<CarLoadDocument> GetCarLoadDocumentById(IUnitOfWork uow, int carLoadDocumentId);
	}
}
