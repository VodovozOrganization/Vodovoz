using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Documents;

namespace Vodovoz.EntityRepositories.Store
{
	public interface ICarLoadDocumentRepository
	{
		decimal LoadedTerminalAmount(IUnitOfWork uow, int routelistId, int terminalId);
		Task<IEnumerable<CarLoadDocument>> GetCarLoadDocumentsById(IUnitOfWork uow, int carLoadDocumentId);
		Task<IEnumerable<CarLoadDocumentItem>> GetAccountableInTrueMarkHavingGtinItemsByCarLoadDocumentId(IUnitOfWork uow, int orderId);
		CarLoadDocumentLoadingProcessAction GetLastLoadingProcessActionByDocumentId(IUnitOfWork uow, int documentId);
	}
}
