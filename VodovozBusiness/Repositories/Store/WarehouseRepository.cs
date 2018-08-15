using System.Collections.Generic;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repository.Store
{
	public static class WarehouseRepository
	{

		public static IList<Warehouse> GetActiveWarehouse(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Warehouse>().WhereNot(x => x.IsArchive).List<Warehouse>();
		}

		public static IList<Warehouse> WarehouseForShipment(IUnitOfWork uow, int routeListId)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null,
				OrderEquipmentNomenclatureAlias = null, 
				resultNomenclatureAlias = null;

			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order>(() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<Vodovoz.Domain.Logistic.RouteListItem>()
				.Where(r => r.RouteList.Id == routeListId)
			    .Where(x => x.WasTransfered == false || (x.WasTransfered && x.NeedToReload))
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderitemsSubqury = QueryOver.Of<OrderItem>(() => orderItemsAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Select(n => n.Nomenclature.Id)
			    .Where(() => OrderItemNomenclatureAlias.NoDelivey == false);
			var orderEquipmentSubquery = QueryOver.Of<OrderEquipment>(() => orderEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderEquipmentAlias.Order, () => orderAlias)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.JoinAlias(e => e.Nomenclature, () => OrderEquipmentNomenclatureAlias)
				.SelectList(list => list.Select(() => OrderEquipmentNomenclatureAlias.Id));

			return uow.Session.QueryOver<Nomenclature>(() => resultNomenclatureAlias)
				.Where(new Disjunction()
					.Add(Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderitemsSubqury))
					.Add(Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderEquipmentSubquery))
				).Where(n => n.Warehouse != null)
				.Select(Projections.Distinct(Projections.Property<Nomenclature>(n => n.Warehouse)))
				.List<Warehouse>();
		}

		public static IList<Warehouse> WarehouseForReception(IUnitOfWork uow, int id)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null, orderNewEquipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null, OrderNewEquipmentNomenclatureAlias = null, resultNomenclatureAlias = null;

			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order>(() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<Vodovoz.Domain.Logistic.RouteListItem>()
				.Where(r => r.RouteList.Id == id)
				.Select(r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty(o => o.Id).In(routeListItemsSubQuery).Select(o => o.Id);

			var orderitemsSubqury = QueryOver.Of<OrderItem>(() => orderItemsAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Select(n => n.Nomenclature.Id);
			var orderEquipmentSubquery = QueryOver.Of<OrderEquipment>(() => orderEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderEquipmentAlias.Order, () => orderAlias)
				.JoinQueryOver(() => orderEquipmentAlias.Equipment)
				.JoinAlias(e => e.Nomenclature, () => OrderEquipmentNomenclatureAlias)
				.SelectList(list => list.Select(() => OrderEquipmentNomenclatureAlias.Id));
			var orderNewEquipmentSubquery = QueryOver.Of<OrderEquipment>(() => orderNewEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderNewEquipmentAlias.Order, () => orderAlias)
				.JoinAlias(() => orderNewEquipmentAlias.Nomenclature, () => OrderNewEquipmentNomenclatureAlias)
				.SelectList(list => list.Select(() => OrderNewEquipmentNomenclatureAlias.Id));

			return uow.Session.QueryOver<Nomenclature>(() => resultNomenclatureAlias)
				.Where(new Disjunction()
					.Add(Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderitemsSubqury))
					.Add(Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderEquipmentSubquery))
					.Add(Subqueries.WhereProperty<Nomenclature>(n => n.Id).In(orderNewEquipmentSubquery))
				).Where(n => n.Warehouse != null)
				.Select(Projections.Distinct(Projections.Property<Nomenclature>(n => n.Warehouse)))
				.List<Warehouse>();
		}
	}
}