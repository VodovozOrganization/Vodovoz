using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Store;

namespace Vodovoz.EntityRepositories.Stock
{
	public class StockRepository : IStockRepository
	{
		public decimal NomenclatureReserved(IUnitOfWork uow, int nomenclatureId)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;

			return uow.Session.QueryOver<Vodovoz.Domain.Orders.Order>(() => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.Where(() => orderItemsAlias.Nomenclature.Id == nomenclatureId)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Accepted)
				.Select(Projections.Sum(() => orderItemsAlias.Count)).SingleOrDefault<decimal>();
		}

		//TODO проверить работу запроса
		public int GetStockForNomenclature(IUnitOfWork uow, int nomenclatureId)
		{
			var stockProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0)"),
				NHibernateUtil.Decimal,
				Projections.Sum<GoodsAccountingOperation>(op => op.Amount));
			
			var queryResult = uow.Session.QueryOver<GoodsAccountingOperation>()
				.Where(op => op.Nomenclature.Id == nomenclatureId)
				.SelectList(list => list
					.SelectGroup(op => op.Nomenclature.Id)
					.Select(stockProjection)
				).SingleOrDefault<object[]>();
				
			return (int)queryResult[1];
		}
		
		//TODO проверить работу запроса
		public Dictionary<int, decimal> NomenclatureInStock(
			IUnitOfWork uow,
			int[] nomenclatureIds,
			int? warehouseId = null,
			DateTime? onDate = null)
		{
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver<WarehouseBulkGoodsAccountingOperation>()
				.JoinAlias(op => op.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Id.IsIn(nomenclatureIds));
				
			if(onDate.HasValue)
			{
				query.And(op => op.OperationTime < onDate.Value);
			}

			if(warehouseId.HasValue)
			{
				query.And(op => op.Warehouse.Id == warehouseId);
			}
			else
			{
				query.And(op => op.Warehouse != null);
			}
			
			var stocklist = query.SelectList(list => list
				   .SelectGroup(() => nomenclatureAlias.Id)
				   .Select(Projections.Sum<WarehouseBulkGoodsAccountingOperation>(op => op.Amount)))
				.TransformUsing(Transformers.AliasToBean<(int nomenclatureId, decimal amount)>())
				.List<(int nomenclatureId, decimal amount)>();
			
			var result = new Dictionary<int, decimal>();
			foreach(var nomenclatureInStock in stocklist)
			{
				result.Add(nomenclatureInStock.nomenclatureId, nomenclatureInStock.amount);
			}

			return result;
		}

		//TODO проверить работу запроса
		public Dictionary<int, decimal> NomenclatureInStock(
			IUnitOfWork uow,
			int warehouseId,
			DateTime? onDate = null,
			NomenclatureCategory? nomenclatureCategory = null,
			ProductGroup nomenclatureType = null,
			Nomenclature nomenclature = null)
		{
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver<WarehouseBulkGoodsAccountingOperation>()
				.JoinAlias(op => op.Nomenclature, () => nomenclatureAlias);
				
			if(nomenclatureCategory != null)
			{
				query.Where(() => nomenclatureAlias.Category == nomenclatureCategory.Value);
			}

			if(nomenclatureType != null)
			{
				query.Where(() => nomenclatureAlias.ProductGroup == nomenclatureType);
			}

			if(nomenclature != null)
			{
				query.Where(() => nomenclatureAlias.Id == nomenclature.Id);
			}
			
			if(onDate.HasValue)
			{
				query.Where(op => op.OperationTime < onDate.Value);
			}
				
			var stocklist =
				query.And(op => op.Warehouse.Id == warehouseId)
					.SelectList(list => list
					   .SelectGroup(() => nomenclatureAlias.Id)
					   .Select(Projections.Sum<WarehouseBulkGoodsAccountingOperation>(op => op.Amount)))
					.TransformUsing(Transformers.AliasToBean<(int nomenclatureId, decimal amount)>())
					.List<(int nomenclatureId, decimal amount)>();
			
			var result = new Dictionary<int, decimal>();
			
			foreach(var nomenclatureInStock in stocklist.Where(x => x.amount != 0))
			{
				result.Add(nomenclatureInStock.nomenclatureId, nomenclatureInStock.amount);
			}

			return result;
		}

		//TODO проверить работу запроса
		public Dictionary<int, decimal> NomenclatureInStock(
			IUnitOfWork uow,
			int storageId,
			OperationType operationType,
			IEnumerable<int> nomenclaturesToInclude,
			IEnumerable<int> nomenclaturesToExclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToInclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToExclude,
			IEnumerable<int> productGroupToInclude,
			IEnumerable<int> productGroupToExclude,
			DateTime? onDate = null)
		{
			Nomenclature nomenclatureAlias = null;

			var query = uow.Session.QueryOver<BulkGoodsAccountingOperation>()
				.Left.JoinAlias(op => op.Nomenclature, () => nomenclatureAlias);

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
				query.Where(x => x.OperationTime < onDate.Value);
			}

			var stocklist = query
				.Where(GoodsAccountingOperationRepository.GetGoodsAccountingOperationCriterionByStorage(operationType, storageId))
				.SelectList(list => list
				   .SelectGroup(() => nomenclatureAlias.Id)
				   .Select(Projections.Sum<BulkGoodsAccountingOperation>(op => op.Amount)))
				.TransformUsing(Transformers.AliasToBean<(int nomenclatureId, decimal amount)>())
				.List<(int nomenclatureId, decimal amount)>();
			
			var result = new Dictionary<int, decimal>();
			
			foreach(var nomenclatureInStock in stocklist.Where(x => x.amount != 0))
			{
				result.Add(nomenclatureInStock.nomenclatureId, nomenclatureInStock.amount);
			}

			return result;
		}

		public Dictionary<int, decimal> NomenclatureInStock(IUnitOfWork uow, int[] warehouseIds, int[] nomenclatureIds)
		{
			var total = new Dictionary<int, decimal>();
			
			foreach(var warehouse in warehouseIds)
			{
				var stockTotal = NomenclatureInStock(uow, nomenclatureIds, warehouse);
				
				foreach(var pair in stockTotal)
				{
					if(total.ContainsKey(pair.Key))
					{
						total[pair.Key] += pair.Value;
					}
					else
					{
						total.Add(pair.Key, pair.Value);
					}
				}
			}
			
			return total;
		}

		//TODO Проверить использование метода и удалить за ненадобностью
		public decimal EquipmentInStock(IUnitOfWork uow, int warehouseId, int equipmentId)
		{
			return EquipmentInStock(uow, warehouseId, new int[] { equipmentId }).Values.FirstOrDefault();
		}

		public Dictionary<int, decimal> EquipmentInStock(IUnitOfWork uow, int warehouseId, int[] equipmentIds)
		{
			/*Equipment equipmentAlias = null;
			GoodsAccountingOperation operationAddAlias = null;
			GoodsAccountingOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<GoodsAccountingOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Equipment.Id == equipmentAlias.Id)
				.And(Restrictions.Eq(Projections.Property<GoodsAccountingOperation>(o => o.IncomingWarehouse.Id), warehouseId))
				.Select(Projections.Sum<GoodsAccountingOperation>(o => o.Amount));

			var subqueryRemove = QueryOver.Of<GoodsAccountingOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Equipment.Id == equipmentAlias.Id)
				.And(Restrictions.Eq(Projections.Property<GoodsAccountingOperation>(o => o.WriteOffWarehouse.Id), warehouseId))
				.Select(Projections.Sum<GoodsAccountingOperation>(o => o.Amount));

			ItemInStock inStock = null;
			
			var stocklist = uow.Session.QueryOver<Equipment>(() => equipmentAlias)
				.Where(() => equipmentAlias.Id.IsIn(equipmentIds))
				.SelectList(list => list
				   .SelectGroup(() => equipmentAlias.Id).WithAlias(() => inStock.Id)
				   .SelectSubQuery(subqueryAdd).WithAlias(() => inStock.Added)
				   .SelectSubQuery(subqueryRemove).WithAlias(() => inStock.Removed)
				).TransformUsing(Transformers.AliasToBean<ItemInStock>()).List<ItemInStock>();
			
			
			
			foreach(var nomenclatureInStock in stocklist)
			{
				result.Add(nomenclatureInStock.Id, nomenclatureInStock.Amount);
			}

			return result;*/
			return new Dictionary<int, decimal>();
		}
	}
}
