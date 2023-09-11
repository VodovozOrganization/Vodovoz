using QS.DomainModel.UoW;
using System.Collections.Generic;

namespace Vodovoz.EntityRepositories.Store
{
	public interface IMovementDocumentRepository
	{
		int GetSendedMovementDocumentsToWarehouseBySubdivision(IUnitOfWork uow, int subdivisionId);
		int GetSendedMovementDocumentsToWarehouseByUserSelectedWarehouses(IUnitOfWork uow, IEnumerable<int> warehousesIds, int currentUserSubdivisionId);
	}
}
