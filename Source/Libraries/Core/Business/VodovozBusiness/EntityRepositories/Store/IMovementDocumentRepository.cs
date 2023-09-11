using QS.DomainModel.UoW;

namespace Vodovoz.EntityRepositories.Store
{
	public interface IMovementDocumentRepository
	{
		int GetSendedMovementDocumentsToWarehouseBySubdivision(IUnitOfWork uow, int subdivisionId);
		int GetSendedMovementDocumentsToWarehouseByUserSelectedWarehouses(IUnitOfWork uow, int[] warehousesIds);
	}
}
