using System;
using QSOrmProject;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain;
using NHibernate.Criterion;
using NHibernate.Transform;
using System.Collections.Generic;
using Vodovoz.Domain.Operations;
using System.Linq;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repository
{
	public static class StockRepository
	{
		public static int NomenclatureReserved(IUnitOfWork uow, Nomenclature nomenclature)
		{
			return NomenclatureReserved(uow, nomenclature.Id);
		}

		public static int NomenclatureReserved(IUnitOfWork uow, int nomenclatureId)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;

			return uow.Session.QueryOver<Vodovoz.Domain.Orders.Order> (() => orderAlias)
				.JoinAlias (() => orderAlias.OrderItems, () => orderItemsAlias)
				.Where (()=>orderItemsAlias.Nomenclature.Id == nomenclatureId)
				.Where(()=>orderAlias.OrderStatus==OrderStatus.Accepted)
				.Select (Projections.Sum (() => orderItemsAlias.Count)).SingleOrDefault<int>();
		}

		public static decimal NomenclatureInStock(IUnitOfWork UoW, Nomenclature nomenclature)
		{
			return NomenclatureInStock(UoW, nomenclature.Warehouse.Id, new int[]{ nomenclature.Id }).Values.FirstOrDefault();
		}			

		public static Dictionary<int,decimal> NomenclatureInStock(IUnitOfWork UoW, int warehouseId, int[] nomenclatureIds)
		{
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<WarehouseMovementOperation> (() => operationAddAlias)
				.Where (() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And (Restrictions.Eq (Projections.Property<WarehouseMovementOperation> (o => o.IncomingWarehouse.Id), warehouseId))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			var subqueryRemove = QueryOver.Of<WarehouseMovementOperation> (() => operationRemoveAlias)
				.Where (() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And (Restrictions.Eq (Projections.Property<WarehouseMovementOperation> (o => o.WriteoffWarehouse.Id), warehouseId))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			ItemInStock inStock = null;
			var stocklist = UoW.Session.QueryOver<Nomenclature> (() => nomenclatureAlias)
				.Where (() => nomenclatureAlias.Id.IsIn (nomenclatureIds))
				.SelectList (list => list
					.SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => inStock.Id)
					.SelectSubQuery (subqueryAdd).WithAlias (() => inStock.Added)
					.SelectSubQuery (subqueryRemove).WithAlias (() => inStock.Removed)
				).TransformUsing (Transformers.AliasToBean<ItemInStock> ()).List<ItemInStock> ();
			var result = new Dictionary<int,decimal> ();
			foreach (var nomenclatureInStock in stocklist) {
				result.Add (nomenclatureInStock.Id, nomenclatureInStock.Amount);
			}
			return result;			      
		}

		public static Dictionary<int,decimal> NomenclatureInStock(IUnitOfWork UoW, int warehouseId)
		{
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<WarehouseMovementOperation> (() => operationAddAlias)
				.Where (() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And (Restrictions.Eq (Projections.Property<WarehouseMovementOperation> (o => o.IncomingWarehouse.Id), warehouseId))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			var subqueryRemove = QueryOver.Of<WarehouseMovementOperation> (() => operationRemoveAlias)
				.Where (() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And (Restrictions.Eq (Projections.Property<WarehouseMovementOperation> (o => o.WriteoffWarehouse.Id), warehouseId))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			ItemInStock inStock = null;
			var stocklist = UoW.Session.QueryOver<Nomenclature> (() => nomenclatureAlias)
				.SelectList (list => list
					.SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => inStock.Id)
					.SelectSubQuery (subqueryAdd).WithAlias (() => inStock.Added)
					.SelectSubQuery (subqueryRemove).WithAlias (() => inStock.Removed)
				).TransformUsing (Transformers.AliasToBean<ItemInStock> ()).List<ItemInStock> ();
			var result = new Dictionary<int,decimal> ();
			foreach (var nomenclatureInStock in stocklist.Where(x => x.Amount != 0)) {
				result.Add (nomenclatureInStock.Id, nomenclatureInStock.Amount);
			}
			return result;			      
		}

		public static decimal EquipmentInStock(IUnitOfWork UoW, int warehouseId, int equipmentId)
		{
			return EquipmentInStock (UoW, warehouseId, new int[]{ equipmentId }).Values.FirstOrDefault ();
		}

		public static Dictionary<int,decimal> EquipmentInStock(IUnitOfWork UoW, int warehouseId, int[] equipmentIds)
		{
			Equipment equipmentAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Equipment.Id == equipmentAlias.Id)
				.And (Restrictions.Eq (Projections.Property<WarehouseMovementOperation> (o => o.IncomingWarehouse.Id), warehouseId))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			var subqueryRemove = QueryOver.Of<WarehouseMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Equipment.Id == equipmentAlias.Id)
				.And (Restrictions.Eq (Projections.Property<WarehouseMovementOperation> (o => o.WriteoffWarehouse.Id), warehouseId))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			ItemInStock inStock = null;
			var stocklist = UoW.Session.QueryOver<Equipment> (() => equipmentAlias)
				.Where (() => equipmentAlias.Id.IsIn (equipmentIds))
				.SelectList (list => list
					.SelectGroup (() => equipmentAlias.Id).WithAlias (() => inStock.Id)
					.SelectSubQuery (subqueryAdd).WithAlias (() => inStock.Added)
					.SelectSubQuery (subqueryRemove).WithAlias (() => inStock.Removed)
				).TransformUsing (Transformers.AliasToBean<ItemInStock>()).List<ItemInStock> ();
			var result = new Dictionary<int,decimal> ();
			foreach(var nomenclatureInStock in stocklist){
				result.Add (nomenclatureInStock.Id, nomenclatureInStock.Amount);
			}
			return result;			      
		}
	}

	class ItemInStock{
		public int Id{ get; set; }
		public decimal Amount{ get{return Added - Removed;}}
		public decimal Added{get;set;}
		public decimal Removed{get;set;}
	}
}

