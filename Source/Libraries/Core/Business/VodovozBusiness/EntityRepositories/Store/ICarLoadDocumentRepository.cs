using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;

namespace Vodovoz.EntityRepositories.Store
{
	public interface ICarLoadDocumentRepository
	{
		decimal LoadedTerminalAmount(IUnitOfWork uow, int routelistId, int terminalId);
		CarLoadDocument GetCarLoadDocumentById(IUnitOfWork uow, int carLoadDocumentId);
	}
}
