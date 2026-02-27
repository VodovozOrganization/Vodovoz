using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;

namespace Vodovoz.Infrastructure.Persistance.Stock
{
	internal sealed class StockRepository : IStockRepository
	{
		public decimal NomenclatureReserved(IUnitOfWork uow, int nomenclatureId)
		{
			Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;

			return uow.Session.QueryOver(() => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.Where(() => orderItemsAlias.Nomenclature.Id == nomenclatureId)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Accepted)
				.Select(Projections.Sum(() => orderItemsAlias.Count)).SingleOrDefault<decimal>();
		}

		public decimal GetStockForNomenclature(IUnitOfWork uow, int nomenclatureId)
		{
			NomenclatureStockNode resultAlias = null;

			var stockProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0)"),
				NHibernateUtil.Decimal,
				Projections.Sum<GoodsAccountingOperation>(op => op.Amount));

			var queryResult = uow.Session.QueryOver<GoodsAccountingOperation>()
				.Where(op => op.Nomenclature.Id == nomenclatureId)
				.SelectList(list => list
					.SelectGroup(op => op.Nomenclature.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(stockProjection).WithAlias(() => resultAlias.Stock)
				)
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockNode>())
				.SingleOrDefault<NomenclatureStockNode>();

			return queryResult.Stock;
		}

		public Dictionary<int, decimal> NomenclatureInStock(
			IUnitOfWork uow,
			int[] nomenclatureIds,
			IEnumerable<int> warehouseIds = null,
			DateTime? onDate = null)
		{
			Nomenclature nomenclatureAlias = null;
			NomenclatureStockNode resultAlias = null;
			WarehouseBulkGoodsAccountingOperation operationAlias = null;

			var query = uow.Session.QueryOver(() => nomenclatureAlias)
				.JoinEntityAlias(
					() => operationAlias,
					() => nomenclatureAlias.Id == operationAlias.Nomenclature.Id,
					JoinType.LeftOuterJoin)
				.Where(() => nomenclatureAlias.Id.IsIn(nomenclatureIds));

			if(onDate.HasValue)
			{
				query.And(() => operationAlias.OperationTime < onDate.Value);
			}

			if(warehouseIds != null && warehouseIds.Any())
			{
				query.AndRestrictionOn(() => operationAlias.Warehouse.Id).IsInG(warehouseIds);
			}

			var stockList = query.SelectList(list => list
				.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				.Select(Projections.Sum(() => operationAlias.Amount)
					.WithAlias(() => resultAlias.Stock))
				)
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockNode>())
				.List<NomenclatureStockNode>();

			return stockList.ToDictionary(x => x.NomenclatureId, x => x.Stock);
		}

		public Dictionary<int, decimal> NomenclatureInStock(
			IUnitOfWork uow,
			int storageId,
			StorageType storageType,
			IEnumerable<int> nomenclaturesToInclude,
			IEnumerable<int> nomenclaturesToExclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToInclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToExclude,
			IEnumerable<int> productGroupToInclude,
			IEnumerable<int> productGroupToExclude,
			DateTime? onDate = null)
		{
			Nomenclature nomenclatureAlias = null;
			WarehouseBulkGoodsAccountingOperation warehouseBulkOperationAlias = null;
			EmployeeBulkGoodsAccountingOperation employeeBulkOperationAlias = null;
			CarBulkGoodsAccountingOperation carBulkOperationAlias = null;
			NomenclatureStockNode resultAlias = null;

			var query = uow.Session.QueryOver(() => nomenclatureAlias);

			IProjection balanceProjection = null;
			IProjection operationTimeProjection = null;

			switch(storageType)
			{
				case StorageType.Employee:
					query.JoinEntityAlias(() => employeeBulkOperationAlias,
							() => nomenclatureAlias.Id == employeeBulkOperationAlias.Nomenclature.Id
								&& employeeBulkOperationAlias.Employee.Id == storageId);

					balanceProjection = Projections.Sum(() => employeeBulkOperationAlias.Amount);
					operationTimeProjection = Projections.Property(() => employeeBulkOperationAlias.OperationTime);
					break;
				case StorageType.Car:
					query.JoinEntityAlias(() => carBulkOperationAlias,
							() => nomenclatureAlias.Id == carBulkOperationAlias.Nomenclature.Id
								&& carBulkOperationAlias.Car.Id == storageId);

					balanceProjection = Projections.Sum(() => carBulkOperationAlias.Amount);
					operationTimeProjection = Projections.Property(() => carBulkOperationAlias.OperationTime);
					break;
				default:
					query.JoinEntityAlias(() => warehouseBulkOperationAlias,
						() => nomenclatureAlias.Id == warehouseBulkOperationAlias.Nomenclature.Id
							&& warehouseBulkOperationAlias.Warehouse.Id == storageId);

					balanceProjection = Projections.Sum(() => warehouseBulkOperationAlias.Amount);
					operationTimeProjection = Projections.Property(() => warehouseBulkOperationAlias.OperationTime);
					break;
			}

			if(nomenclatureTypeToInclude != null && nomenclatureTypeToInclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.Category).IsInG(nomenclatureTypeToInclude);
			}

			if(productGroupToInclude != null && productGroupToInclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.ProductGroup.Id).IsInG(productGroupToInclude);
			}

			if(nomenclaturesToInclude != null && nomenclaturesToInclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.Id).IsInG(nomenclaturesToInclude);
			}

			if(nomenclatureTypeToExclude != null && nomenclatureTypeToExclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.Category).Not.IsInG(nomenclatureTypeToExclude);
			}

			if(productGroupToExclude != null && productGroupToExclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.ProductGroup).Not.IsInG(productGroupToExclude);
			}

			if(nomenclaturesToExclude != null && nomenclaturesToExclude.Any())
			{
				query.WhereRestrictionOn(() => nomenclatureAlias.Id).Not.IsInG(nomenclaturesToExclude);
			}

			if(onDate.HasValue)
			{
				query.Where(Restrictions.Lt(operationTimeProjection, onDate.Value));
			}

			var result = query
				.SelectList(list => list
				   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
				   .Select(balanceProjection).WithAlias(() => resultAlias.Stock))
				.Where(Restrictions.Gt(balanceProjection, 0))
				.TransformUsing(Transformers.AliasToBean<NomenclatureStockNode>())
				.List<NomenclatureStockNode>();

			return result.ToDictionary(x => x.NomenclatureId, x => x.Stock);
		}
	}
}
