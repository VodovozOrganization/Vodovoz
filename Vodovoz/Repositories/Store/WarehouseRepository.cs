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

		public static Dictionary<int,decimal> NomenclaturesInStock(IUnitOfWork UoW, Warehouse warehouse,int[] nomenclatureIds)
		{
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;

			var subqueryAdd = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And (Restrictions.Eq (Projections.Property<WarehouseMovementOperation> (o => o.IncomingWarehouse), warehouse))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			var subqueryRemove = QueryOver.Of<WarehouseMovementOperation>(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.And (Restrictions.Eq (Projections.Property<WarehouseMovementOperation> (o => o.WriteoffWarehouse), warehouse))
				.Select (Projections.Sum<WarehouseMovementOperation> (o => o.Amount));

			NomenclatureInStock inStock = null;
			var stocklist = UoW.Session.QueryOver<Nomenclature> (() => nomenclatureAlias).Where (() => nomenclatureAlias.Id.IsIn (nomenclatureIds))
				.SelectList (list => list
					.SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => inStock.Id)
					.SelectSubQuery (subqueryAdd).WithAlias (() => inStock.Added)
					.SelectSubQuery (subqueryRemove).WithAlias (() => inStock.Removed)
				).TransformUsing (Transformers.AliasToBean<NomenclatureInStock>()).List<NomenclatureInStock> ();
			var result = new Dictionary<int,decimal> ();
			foreach(var nomenclatureInStock in stocklist){
				result.Add (nomenclatureInStock.Id, nomenclatureInStock.Amount);
			}
			return result;			      
		}
	}
	class NomenclatureInStock{
		public int Id{ get; set; }
		public decimal Amount{ get{return Added - Removed;}}
		public decimal Added{get;set;}
		public decimal Removed{get;set;}
	}
}

