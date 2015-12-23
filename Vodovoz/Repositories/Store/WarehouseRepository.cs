using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Logistic;
using System.Linq;
using System.Collections;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Repository.Store
{
	public static class WarehouseRepository
	{
		public static IList<Warehouse> GetActiveWarehouse(IUnitOfWork uow)
		{
			return uow.Session.CreateCriteria<Warehouse> ().List<Warehouse> ();
		}

		public static IList<Warehouse> WarehouseForShipment(IUnitOfWork uow, ShipmentDocumentType type, int id)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null, resultNomenclatureAlias = null;

			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order> (() => orderAlias);

			switch (type) {
			case ShipmentDocumentType.Order:
				ordersQuery.Where (o => o.Id == id)
					.Select (o => o.Id);
				break;
			case ShipmentDocumentType.RouteList:
				var routeListItemsSubQuery = QueryOver.Of<Vodovoz.Domain.Logistic.RouteListItem> ()
					.Where (r => r.RouteList.Id == id)
					.Select (r => r.Order.Id);
				ordersQuery.WithSubquery.WhereProperty (o => o.Id).In (routeListItemsSubQuery).Select(o=>o.Id);
				break;
			default:
				throw new NotSupportedException (type.ToString ());
			}

			var orderitemsSubqury = QueryOver.Of<OrderItem> (() => orderItemsAlias)
				.WithSubquery.WhereProperty (i => i.Order.Id).In (ordersQuery)
				.JoinAlias (() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Select (n => n.Nomenclature.Id );
			var orderEquipmentSubquery = QueryOver.Of<OrderEquipment> (() => orderEquipmentAlias)
				.WithSubquery.WhereProperty (i => i.Order.Id).In (ordersQuery)
				.JoinAlias (() => orderEquipmentAlias.Order, () => orderAlias)
				.JoinQueryOver (() => orderEquipmentAlias.Equipment)
				.Where (() => orderEquipmentAlias.Direction == Direction.Deliver)
				.JoinAlias (e => e.Nomenclature, () => OrderEquipmentNomenclatureAlias)
				.SelectList (list => list.Select (() => OrderEquipmentNomenclatureAlias.Id));

			return uow.Session.QueryOver<Nomenclature> (() => resultNomenclatureAlias)
				.Where (new Disjunction ()
					.Add (Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderitemsSubqury))
					.Add (Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderEquipmentSubquery))
				).Where (n => n.Warehouse != null)
				.Select (Projections.Distinct(Projections.Property<Nomenclature> (n => n.Warehouse)))
				.List<Warehouse> ();
		}

		public static List<Warehouse> WarehousesNotLoadedFrom(IUnitOfWork uow,ShipmentDocumentType type, int id)
		{			
			var visitedWarehousesIds = WarehousesLoadedFrom (uow, type, id).Select (warehouse => warehouse.Id).ToList ();				
			return WarehouseForShipment (uow, type, id).Where (warehouse => !visitedWarehousesIds.Contains (warehouse.Id)).ToList();
		}

		public static IList<Warehouse> WarehousesLoadedFrom(IUnitOfWork uow,ShipmentDocumentType type, int id)
		{
			Warehouse warehouseAlias = null;
			var cardocumentsQuery = uow.Session.QueryOver<CarLoadDocument> ();
			switch (type) {
			case ShipmentDocumentType.Order:
				cardocumentsQuery.Where (doc => doc.Order.Id == id);
				break;
			case ShipmentDocumentType.RouteList:
				cardocumentsQuery.Where (doc => doc.RouteList.Id == id);
				break;
			default:
				throw new NotSupportedException (type.ToString ());
			}	
			return cardocumentsQuery
				.JoinAlias (doc => doc.Warehouse, () => warehouseAlias)
				.Select (doc => doc.Warehouse)
				.List<Warehouse> ();
		}

		public static decimal NomenclatureInStock(IUnitOfWork UoW, int warehouseId, int nomenclatureId)
		{
			return NomenclatureInStock (UoW, warehouseId, new int[]{ nomenclatureId }).Values.FirstOrDefault ();
		}

		public static Dictionary<int,decimal> NomenclatureInStock(IUnitOfWork UoW,int warehouseId, int[] nomenclatureIds)
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

