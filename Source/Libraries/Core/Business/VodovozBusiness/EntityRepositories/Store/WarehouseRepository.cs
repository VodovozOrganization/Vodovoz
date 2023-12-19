using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.EntityRepositories.Store
{
	public class WarehouseRepository : IWarehouseRepository
	{
		public IList<Warehouse> GetActiveWarehouse(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Warehouse>().WhereNot(x => x.IsArchive).List<Warehouse>();
		}

		public IList<Warehouse> WarehousesForPublishOnlineStore(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Warehouse>()
					  .WhereNot(x => x.IsArchive)
					  .Where(x => x.PublishOnlineStore)
					  .List<Warehouse>();
		}

		public IEnumerable<NomenclatureStockNode> GetWarehouseNomenclatureStock(
			IUnitOfWork uow, OperationType operationType, int storageId, IEnumerable<int> nomenclatureIds)
		{
			NomenclatureStockNode resultAlias = null;
			Nomenclature nomenclatureAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			EmployeeBulkGoodsAccountingOperation employeeBulkOperationAlias = null;
			CarBulkGoodsAccountingOperation carBulkOperationAlias = null;
			
			var query = uow.Session.QueryOver(() => nomenclatureAlias);
			IProjection nomenclatureBalance = null;

			if(operationType == OperationType.WarehouseBulkGoodsAccountingOperation)
			{
				query.JoinEntityAlias(() => warehouseBulkOperationAlias,
						() => warehouseBulkOperationAlias.Nomenclature.Id == nomenclatureAlias.Id,
						JoinType.LeftOuterJoin)
					.Where(() => warehouseBulkOperationAlias.Warehouse.Id == storageId);

				nomenclatureBalance = Projections.Sum(() => warehouseBulkOperationAlias.Amount);
			}
			else if(operationType == OperationType.EmployeeBulkGoodsAccountingOperation)
			{
				query.JoinEntityAlias(() => employeeBulkOperationAlias,
					() => employeeBulkOperationAlias.Nomenclature.Id == nomenclatureAlias.Id,
					JoinType.LeftOuterJoin)
					.Where(() => employeeBulkOperationAlias.Employee.Id == storageId);
				
				nomenclatureBalance = Projections.Sum(() => employeeBulkOperationAlias.Amount);
			}
			else if(operationType == OperationType.CarBulkGoodsAccountingOperation)
			{
				query.JoinEntityAlias(() => carBulkOperationAlias,
					() => carBulkOperationAlias.Nomenclature.Id == nomenclatureAlias.Id,
					JoinType.LeftOuterJoin)
					.Where(() => carBulkOperationAlias.Car.Id == storageId);
				
				nomenclatureBalance = Projections.Sum(() => carBulkOperationAlias.Amount);
			}
			
			var stockProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0)"),
				NHibernateUtil.Decimal,
				nomenclatureBalance);
			
			return query.AndRestrictionOn(() => nomenclatureAlias.Id).IsIn(nomenclatureIds.ToArray())
				.And(() => !nomenclatureAlias.HasInventoryAccounting)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(stockProjection).WithAlias(() => resultAlias.Stock))
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockNode>())
				.List<NomenclatureStockNode>();
		}

		public IEnumerable<Nomenclature> GetDiscrepancyNomenclatures(IUnitOfWork uow, int warehouseId)
		{
			if(uow == null) {
				throw new ArgumentNullException(nameof(uow));
			}

			Nomenclature nomenclatureAlias = null;
			MovementDocument movementDocumentAlias = null;
			MovementDocumentItem movementDocumentItemAlias = null;

			return uow.Session.QueryOver(() => movementDocumentItemAlias)
				.Left.JoinAlias(() => movementDocumentItemAlias.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(() => movementDocumentItemAlias.Document, () => movementDocumentAlias)
				.Where(() => movementDocumentAlias.Status == MovementDocumentStatus.Discrepancy)
				.Where(() => movementDocumentAlias.FromWarehouse.Id == warehouseId)
				.Where(() => movementDocumentItemAlias.SentAmount != movementDocumentItemAlias.ReceivedAmount)
				.Select(Projections.Entity(() => nomenclatureAlias))
				.List<Nomenclature>();
		}

		public bool WarehouseByMovementDocumentsNotificationsSubdivisionExists(IUnitOfWork uow, int subdivisionId)
		{
			return uow.Session.QueryOver<Warehouse>()
				.Where(w => w.MovementDocumentsNotificationsSubdivisionRecipient.Id == subdivisionId)
				.List()
				.Any();
		}
		
		public int GetTotalShippedKgByWarehousesAndProductGroups(
			IUnitOfWork uow, DateTime dateFrom, DateTime dateTo, IEnumerable<int> productGroupsIds, IEnumerable<int> warehousesIds)
		{
			NomenclatureTotalShippedKg resultAlias = null;
			Warehouse warehouseAlias = null;
			Nomenclature nomenclatureAlias = null;

			var result = uow.Session.QueryOver<WarehouseBulkGoodsAccountingOperation>()
				.JoinAlias(wmo => wmo.Warehouse, () => warehouseAlias)
				.JoinAlias(wmo => wmo.Nomenclature, () => nomenclatureAlias)
				.WhereRestrictionOn(() => nomenclatureAlias.ProductGroup.Id).IsInG(productGroupsIds)
				.AndRestrictionOn(() => warehouseAlias.Id).IsInG(warehousesIds)
				.And(wmo => wmo.OperationTime >= dateFrom && wmo.OperationTime < dateTo)
				.And(wmo => wmo.Amount < 0)
				.SelectList(list => list
					.SelectGroup(x => x.Nomenclature.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Int32, "-?1 * ?2"),
						NHibernateUtil.Int32,
						Projections.Sum<WarehouseBulkGoodsAccountingOperation>(wmo => wmo.Amount),
						Projections.Property(() => nomenclatureAlias.Weight))).WithAlias(() => resultAlias.TotalShippedKg))
				.TransformUsing(Transformers.AliasToBean<NomenclatureTotalShippedKg>())
				.List<NomenclatureTotalShippedKg>();

			return result.Sum(x => x.TotalShippedKg);
		}

		public class NomenclatureTotalShippedKg
		{
			public int NomenclatureId { get; set; }
			public int TotalShippedKg { get; set; }
		}
	}
}
