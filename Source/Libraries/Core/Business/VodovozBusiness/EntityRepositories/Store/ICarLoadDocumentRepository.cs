using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.EntityRepositories.Store
{
	public interface ICarLoadDocumentRepository
	{
		decimal LoadedTerminalAmount(IUnitOfWork uow, int routelistId, int terminalId);
		Task<IEnumerable<CarLoadDocumentEntity>> GetCarLoadDocumentsById(IUnitOfWork uow, int carLoadDocumentId);
		Task<IEnumerable<CarLoadDocumentItemEntity>> GetAccountableInTrueMarkHavingGtinItemsByCarLoadDocumentId(IUnitOfWork uow, int orderId);
		CarLoadDocumentLoadingProcessAction GetLastLoadingProcessActionByDocumentId(IUnitOfWork uow, int documentId);
	}
}
