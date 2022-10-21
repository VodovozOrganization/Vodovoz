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

		public int GetStockForNomenclature(IUnitOfWork uow, int nomenclatureId)
		{
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And(() => operationAddAlias.IncomingWarehouse.Id != null)
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryRemove = QueryOver.Of<WarehouseMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And(() => operationRemoveAlias.WriteoffWarehouse.Id != null)
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var amountProjection = Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Int32, "( ?1 - ?2 )"),
				NHibernateUtil.Int32, new IProjection[] {
					Projections.SubQuery(subqueryAdd),
					Projections.SubQuery(subqueryRemove)
				}
			);

			ItemInStock inStock = null;
			var queryResult = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Id == nomenclatureId)
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id)
					.Select(amountProjection)
				).SingleOrDefault<object[]>();
				
			return (int)queryResult[1];
		}
		
		public Dictionary<int, decimal> NomenclatureInStock(IUnitOfWork uow, int[] nomenclatureIds, int? warehouseId = null, DateTime? onDate = null)
		{
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id);
			if(onDate.HasValue)
			{
				subqueryAdd.Where(x => x.OperationTime < onDate.Value);
			}

			if(warehouseId.HasValue)
			{
				subqueryAdd.And(() => operationAddAlias.IncomingWarehouse.Id == warehouseId);
			}
			else
			{
				subqueryAdd.And(() => operationAddAlias.IncomingWarehouse != null);
			}
			subqueryAdd.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryRemove = QueryOver.Of<WarehouseMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id);
			if(onDate.HasValue)
			{
				subqueryRemove.Where(x => x.OperationTime < onDate.Value);
			}

			if(warehouseId.HasValue)
			{
				subqueryRemove.And(() => operationRemoveAlias.WriteoffWarehouse.Id == warehouseId);
			}
			else
			{
				subqueryRemove.And(() => operationRemoveAlias.WriteoffWarehouse != null);
			}
			subqueryRemove.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			ItemInStock inStock = null;
			var stocklist = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Id.IsIn(nomenclatureIds))
				.SelectList(list => list
				   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => inStock.Id)
				   .SelectSubQuery(subqueryAdd).WithAlias(() => inStock.Added)
				   .SelectSubQuery(subqueryRemove).WithAlias(() => inStock.Removed)
				).TransformUsing(Transformers.AliasToBean<ItemInStock>()).List<ItemInStock>();
			var result = new Dictionary<int, decimal>();
			foreach(var nomenclatureInStock in stocklist)
				result.Add(nomenclatureInStock.Id, nomenclatureInStock.Amount);

			return result;
		}

		public Dictionary<int, decimal> NomenclatureInStock(IUnitOfWork uow, int warehouseId, DateTime? onDate = null, NomenclatureCategory? nomenclatureCategory = null, ProductGroup nomenclatureType = null , Nomenclature nomenclature = null)
		{
			Nomenclature nomenclatureAlias = null;
			Nomenclature nomenclatureAddOperationAlias = null;
			Nomenclature nomenclatureRemoveOperationAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.JoinAlias(() => operationAddAlias.Nomenclature, () => nomenclatureAddOperationAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id);
			
			if(nomenclatureCategory != null)
			{
				subqueryAdd.Where(() => nomenclatureAddOperationAlias.Category == nomenclatureCategory.Value);
			}

			if(nomenclatureType != null)
			{
				subqueryAdd.Where(() => nomenclatureAddOperationAlias.ProductGroup.Id == nomenclatureType.Id);
			}

			if(nomenclature != null)
			{
				subqueryAdd.Where(() => nomenclatureAddOperationAlias.Id == nomenclature.Id);
			}

			if(onDate.HasValue)
			{
				subqueryAdd.Where(x => x.OperationTime < onDate.Value);
			}

			subqueryAdd.And(Restrictions.Eq(Projections.Property<WarehouseMovementOperation>(o => o.IncomingWarehouse.Id), warehouseId))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryRemove = QueryOver.Of(() => operationRemoveAlias)
				.JoinAlias(() => operationRemoveAlias.Nomenclature, () => nomenclatureRemoveOperationAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id);
			
			if(nomenclatureCategory != null)
			{
				subqueryRemove.Where(() => nomenclatureRemoveOperationAlias.Category == nomenclatureCategory.Value);
			}

			if(nomenclatureType != null)
			{
				subqueryRemove.Where(() => nomenclatureRemoveOperationAlias.ProductGroup == nomenclatureType);
			}

			if(nomenclature != null)
			{
				subqueryRemove.Where(() => nomenclatureRemoveOperationAlias.Id == nomenclature.Id);
			}

			if(onDate.HasValue)
			{
				subqueryRemove.Where(x => x.OperationTime < onDate.Value);
			}

			subqueryRemove.And(Restrictions.Eq(Projections.Property<WarehouseMovementOperation>(o => o.WriteoffWarehouse.Id), warehouseId))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			ItemInStock inStock = null;
			
			var stocklist = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
				.SelectList(list => list
				   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => inStock.Id)
				   .SelectSubQuery(subqueryAdd).WithAlias(() => inStock.Added)
				   .SelectSubQuery(subqueryRemove).WithAlias(() => inStock.Removed)
				).TransformUsing(Transformers.AliasToBean<ItemInStock>()).List<ItemInStock>();
			
			var result = new Dictionary<int, decimal>();
			
			foreach(var nomenclatureInStock in stocklist.Where(x => x.Amount != 0))
			{
				result.Add(nomenclatureInStock.Id, nomenclatureInStock.Amount);
			}

			return result;
		}

		public Dictionary<int, decimal> NomenclatureInStock(
			IUnitOfWork uow, int warehouseId,
			int[] nomenclaturesToInclude,
			int[] nomenclaturesToExclude,
			string[] nomenclatureTypeToInclude,
			string[] nomenclatureTypeToExclude,
			int[] productGroupToInclude,
			int[] productGroupToExclude,
			DateTime? onDate = null)
		{
			Nomenclature nomenclatureAlias = null;
			Nomenclature nomenclatureAddOperationAlias = null;
			Nomenclature nomenclatureRemoveOperationAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.JoinAlias(() => operationAddAlias.Nomenclature, () => nomenclatureAddOperationAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id);

			if (nomenclatureTypeToInclude.Length > 0)
			{
				List<NomenclatureCategory> parsedCategories = new List<NomenclatureCategory>();
				
				foreach (var categoryName in nomenclatureTypeToInclude)
				{
					parsedCategories.Add((NomenclatureCategory)Enum.Parse(typeof(NomenclatureCategory), categoryName));
				}
				
				subqueryAdd.Where(() => nomenclatureAddOperationAlias.Category.IsIn(parsedCategories));
			}
			
			if(productGroupToInclude != null && productGroupToInclude.Any())
			{
				subqueryAdd.Where(() => nomenclatureAddOperationAlias.ProductGroup.Id.IsIn(productGroupToInclude));
			}

			if(nomenclaturesToInclude != null && nomenclaturesToInclude.Any())
			{
				subqueryAdd.Where(() => nomenclatureAddOperationAlias.Id.IsIn(nomenclaturesToInclude));
			}

			if(nomenclatureTypeToExclude.Length > 0)
			{
				List<NomenclatureCategory> parsedCategories = new List<NomenclatureCategory>();
				
				foreach(var categoryName in nomenclatureTypeToExclude)
				{
					parsedCategories.Add((NomenclatureCategory)Enum.Parse(typeof(NomenclatureCategory), categoryName));
				}
				
				subqueryAdd.Where(() => nomenclatureAddOperationAlias.Category.IsIn(parsedCategories));
			}
			
			if(productGroupToExclude != null && productGroupToExclude.Any())
			{
				subqueryAdd.Where(() => nomenclatureAddOperationAlias.ProductGroup.IsIn(productGroupToExclude));
			}

			if(nomenclaturesToExclude != null && nomenclaturesToExclude.Any())
			{
				subqueryAdd.Where(() => nomenclatureAddOperationAlias.Id.IsIn(nomenclaturesToExclude));
			}

			if(onDate.HasValue)
			{
				subqueryAdd.Where(x => x.OperationTime < onDate.Value);
			}

			subqueryAdd.And(Restrictions.Eq(Projections.Property<WarehouseMovementOperation>(o => o.IncomingWarehouse.Id), warehouseId))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryRemove = QueryOver.Of(() => operationRemoveAlias)
				.JoinAlias(() => operationRemoveAlias.Nomenclature, () => nomenclatureRemoveOperationAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id);

			if(nomenclatureTypeToInclude.Length > 0)
			{
				List<NomenclatureCategory> parsedCategories = new List<NomenclatureCategory>();
				
				foreach(var categoryName in nomenclatureTypeToInclude)
				{
					parsedCategories.Add((NomenclatureCategory)Enum.Parse(typeof(NomenclatureCategory), categoryName));
				}
				
				subqueryRemove.Where(() => nomenclatureRemoveOperationAlias.Category.IsIn(parsedCategories));
			}
			
			if(productGroupToInclude != null && productGroupToInclude.Any())
			{
				subqueryRemove.Where(() => nomenclatureRemoveOperationAlias.ProductGroup.Id.IsIn(productGroupToInclude));
			}

			if(nomenclaturesToInclude != null && nomenclaturesToInclude.Any())
			{
				subqueryRemove.Where(() => nomenclatureRemoveOperationAlias.Id.IsIn(nomenclaturesToInclude));
			}

			if (nomenclatureTypeToExclude.Length > 0)
			{
				List<NomenclatureCategory> parsedCategories = new List<NomenclatureCategory>();
				
				foreach (var categoryName in nomenclatureTypeToExclude)
				{
					parsedCategories.Add((NomenclatureCategory)Enum.Parse(typeof(NomenclatureCategory), categoryName));
				}
				
				subqueryRemove.Where(() => nomenclatureRemoveOperationAlias.Category.IsIn(parsedCategories));
			}
			
			if(productGroupToExclude != null && productGroupToExclude.Any())
			{
				subqueryRemove.Where(() => nomenclatureRemoveOperationAlias.ProductGroup.IsIn(productGroupToExclude));
			}

			if(nomenclaturesToExclude != null && nomenclaturesToExclude.Any())
			{
				subqueryRemove.Where(() => nomenclatureRemoveOperationAlias.Id.IsIn(nomenclaturesToExclude));
			}

			if(onDate.HasValue)
			{
				subqueryRemove = subqueryRemove.Where(x => x.OperationTime < onDate.Value);
			}

			subqueryRemove.And(Restrictions.Eq(Projections.Property<WarehouseMovementOperation>(o => o.WriteoffWarehouse.Id), warehouseId))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			ItemInStock inStock = null;
			
			var stocklist = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
				.SelectList(list => list
				   .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => inStock.Id)
				   .SelectSubQuery(subqueryAdd).WithAlias(() => inStock.Added)
				   .SelectSubQuery(subqueryRemove).WithAlias(() => inStock.Removed)
				).TransformUsing(Transformers.AliasToBean<ItemInStock>()).List<ItemInStock>();
			
			var result = new Dictionary<int, decimal>();
			
			foreach(var nomenclatureInStock in stocklist.Where(x => x.Amount != 0))
			{
				result.Add(nomenclatureInStock.Id, nomenclatureInStock.Amount);
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

		public decimal EquipmentInStock(IUnitOfWork uow, int warehouseId, int equipmentId)
		{
			return EquipmentInStock(uow, warehouseId, new int[] { equipmentId }).Values.FirstOrDefault();
		}

		public Dictionary<int, decimal> EquipmentInStock(IUnitOfWork uow, int warehouseId, int[] equipmentIds)
		{
			Equipment equipmentAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Equipment.Id == equipmentAlias.Id)
				.And(Restrictions.Eq(Projections.Property<WarehouseMovementOperation>(o => o.IncomingWarehouse.Id), warehouseId))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryRemove = QueryOver.Of<WarehouseMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Equipment.Id == equipmentAlias.Id)
				.And(Restrictions.Eq(Projections.Property<WarehouseMovementOperation>(o => o.WriteoffWarehouse.Id), warehouseId))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			ItemInStock inStock = null;
			
			var stocklist = uow.Session.QueryOver<Equipment>(() => equipmentAlias)
				.Where(() => equipmentAlias.Id.IsIn(equipmentIds))
				.SelectList(list => list
				   .SelectGroup(() => equipmentAlias.Id).WithAlias(() => inStock.Id)
				   .SelectSubQuery(subqueryAdd).WithAlias(() => inStock.Added)
				   .SelectSubQuery(subqueryRemove).WithAlias(() => inStock.Removed)
				).TransformUsing(Transformers.AliasToBean<ItemInStock>()).List<ItemInStock>();
			
			var result = new Dictionary<int, decimal>();
			
			foreach(var nomenclatureInStock in stocklist)
			{
				result.Add(nomenclatureInStock.Id, nomenclatureInStock.Amount);
			}

			return result;
		}
	}
}
