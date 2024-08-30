using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Domain.Documents;
using VodovozBusiness.Domain.Documents;

namespace Vodovoz.EntityRepositories.Store
{
	public interface ICarLoadDocumentRepository
	{
		decimal LoadedTerminalAmount(IUnitOfWork uow, int routelistId, int terminalId);
		IQueryable<CarLoadDocument> GetCarLoadDocumentsById(IUnitOfWork uow, int carLoadDocumentId);
		IQueryable<CarLoadDocumentItem> GetWaterItemsInCarLoadDocumentById(IUnitOfWork uow, int orderId);
		IQueryable<CarLoadDocumentLoadingProcessAction> GetLoadingProcessActionsByDocumentId(IUnitOfWork uow, int documentId);
	}
}
