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
			MovementDocument movementDocumentAlias = null;
			Warehouse warehouseAlias = null;

			return uow.Session.QueryOver(() => movementDocumentAlias)
				//.JoinEntityAlias(() => warehouseAlias,
				//	() => warehouseAlias.MovementDocumentsNotificationsSubdivisionRecipient.Id == currentUserSubdivisionId)
				.Where(md => md.Status == MovementDocumentStatus.Sended)
				.And(md => md.ToWarehouse.Id != 13)// warehouseAlias.Id)
				.AndRestrictionOn(() => movementDocumentAlias.ToWarehouse.Id).IsInG(warehousesIds)
				.Select(Projections.Count(Projections.Id()))
				.SingleOrDefault<int>();
		}
	}
}
