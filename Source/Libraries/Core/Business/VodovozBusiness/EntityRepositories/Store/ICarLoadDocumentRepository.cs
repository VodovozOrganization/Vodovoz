using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.EntityRepositories.Store
{
	public interface ICarLoadDocumentRepository
	{
		decimal LoadedTerminalAmount(IUnitOfWork uow, int routelistId, int terminalId);
		IQueryable<CarLoadDocumentEntity> GetCarLoadDocumentsById(IUnitOfWork uow, int carLoadDocumentId);
		IQueryable<CarLoadDocumentItemEntity> GetWaterItemsInCarLoadDocumentById(IUnitOfWork uow, int orderId);
		IQueryable<CarLoadDocumentLoadingProcessAction> GetLoadingProcessActionsByDocumentId(IUnitOfWork uow, int documentId);
	}
}
