using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Store;
using NHibernate;
using NHibernate.Criterion;
using Vodovoz.Domain.Documents.MovementDocuments;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.EntityRepositories.Store
{
	public class MovementDocumentRepository : IMovementDocumentRepository
	{
		public int GetSendedMovementDocumentsToWarehouseBySubdivision(IUnitOfWork uow, int subdivisionId)
		{
			Warehouse warehouseAlias = null;

			return uow.Session.QueryOver<MovementDocument>()
				.JoinEntityAlias(() => warehouseAlias,
					() => warehouseAlias.MovementDocumentsNotificationsSubdivisionRecipient.Id == subdivisionId)
				.Where(md => md.Status == MovementDocumentStatus.Sended)
				.And(md => md.ToWarehouse.Id == warehouseAlias.Id)
				.Select(Projections.Count(Projections.Id()))
				.SingleOrDefault<int>();
		}

		public int GetSendedMovementDocumentsToWarehouseByUserSelectedWarehouses(IUnitOfWork uow, IEnumerable<int> warehousesIds, int currentUserSubdivisionId)
		{
			var subdivisionWarehouses = uow.GetAll<Warehouse>()
				.Where(w => w.MovementDocumentsNotificationsSubdivisionRecipient.Id == currentUserSubdivisionId)
				.Select(w => w.Id);

			var docsCount = uow.GetAll<MovementDocument>()
				.Where(d => 
					d.Status == MovementDocumentStatus.Sended
					&& warehousesIds.Contains(d.ToWarehouse.Id)
					&& !subdivisionWarehouses.Contains(d.ToWarehouse.Id))
				.Select(d => d.Id)
				.Distinct()
				.Count();

			return docsCount;
		}
	}
}
