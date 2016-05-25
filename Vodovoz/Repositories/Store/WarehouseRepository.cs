using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;

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

		public static IList<Warehouse> WarehouseForShipment(IUnitOfWork uow, int id)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null, resultNomenclatureAlias = null;

			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order> (() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<Vodovoz.Domain.Logistic.RouteListItem> ()
				.Where (r => r.RouteList.Id == id)
				.Select (r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty (o => o.Id).In (routeListItemsSubQuery).Select(o=>o.Id);

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

		public static IList<Warehouse> WarehouseForReception(IUnitOfWork uow, int id)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null, orderNewEquipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null, OrderNewEquipmentNomenclatureAlias = null, resultNomenclatureAlias = null;

			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order> (() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<Vodovoz.Domain.Logistic.RouteListItem> ()
				.Where (r => r.RouteList.Id == id)
				.Select (r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty (o => o.Id).In (routeListItemsSubQuery).Select(o=>o.Id);

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

		public static List<Warehouse> WarehousesNotLoadedFrom(IUnitOfWork uow, int id)
		{			
			var visitedWarehousesIds = WarehousesLoadedFrom (uow, id).Select (warehouse => warehouse.Id).ToList ();				
			return WarehouseForShipment (uow, id).Where (warehouse => !visitedWarehousesIds.Contains (warehouse.Id)).ToList();
		}

		public static IList<Warehouse> WarehousesLoadedFrom(IUnitOfWork uow, int id)
		{
			Warehouse warehouseAlias = null;
			var cardocumentsQuery = uow.Session.QueryOver<CarLoadDocument> ();
			cardocumentsQuery.Where (doc => doc.RouteList.Id == id);

			return cardocumentsQuery
				.JoinAlias (doc => doc.Warehouse, () => warehouseAlias)
				.Select (doc => doc.Warehouse)
				.List<Warehouse> ();
		}
	}
}