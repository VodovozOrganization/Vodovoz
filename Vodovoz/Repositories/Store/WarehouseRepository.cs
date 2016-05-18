using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using System.Linq;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Repository.Store
{
	public static class WarehouseRepository
	{
		public static IList<Warehouse> GetActiveWarehouse(IUnitOfWork uow)
		{
			return uow.Session.CreateCriteria<Warehouse> ().List<Warehouse> ();
		}

		public static QueryOver<Warehouse> ActiveWarehouseQuery()
		{
			return QueryOver.Of<Warehouse>();
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

		public static IList<Warehouse> WarehouseForReception(IUnitOfWork uow, ShipmentDocumentType type, int id)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null, orderNewEquipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null, OrderNewEquipmentNomenclatureAlias = null, resultNomenclatureAlias = null;

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
				.JoinAlias (e => e.Nomenclature, () => OrderEquipmentNomenclatureAlias)
				.SelectList (list => list.Select (() => OrderEquipmentNomenclatureAlias.Id));
			var orderNewEquipmentSubquery = QueryOver.Of<OrderEquipment> (() => orderNewEquipmentAlias)
				.WithSubquery.WhereProperty (i => i.Order.Id).In (ordersQuery)
				.JoinAlias (() => orderNewEquipmentAlias.Order, () => orderAlias)
				.JoinAlias (() => orderNewEquipmentAlias.NewEquipmentNomenclature, () => OrderNewEquipmentNomenclatureAlias)
				.SelectList (list => list.Select (() => OrderNewEquipmentNomenclatureAlias.Id));

			return uow.Session.QueryOver<Nomenclature> (() => resultNomenclatureAlias)
				.Where (new Disjunction ()
					.Add (Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderitemsSubqury))
					.Add (Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderEquipmentSubquery))
					.Add (Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderNewEquipmentSubquery))
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
	}
}

