using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Repository.Logistics
{
	public static class RouteListRepository
	{
		public static IList<RouteList> GetDriverRouteLists(IUnitOfWork uow, Employee driver)
		{
			RouteList routeListAlias = null;

			return uow.Session.QueryOver<RouteList> (() => routeListAlias)
				.Where (() => routeListAlias.Driver == driver)
				.Where (() => routeListAlias.Status == RouteListStatus.EnRoute)
				.List ();
		}

		public static IList<GoodsInRouteListResult> GetGoodsInRLWithoutEquipments (IUnitOfWork uow, RouteList routeList, Warehouse warehouse = null)
		{
			GoodsInRouteListResult resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null;

			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order> (() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<RouteListItem> ()
				.Where (r => r.RouteList.Id == routeList.Id)
				.Select (r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty (o => o.Id).In (routeListItemsSubQuery).Select (o => o.Id);

			var orderitemsQuery = uow.Session.QueryOver<OrderItem>(() => orderItemsAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Where(() => !OrderItemNomenclatureAlias.Serial);
			if (warehouse != null)
				orderitemsQuery.Where(() => OrderItemNomenclatureAlias.Warehouse == warehouse);
			
			return orderitemsQuery.SelectList (list => list
				.SelectGroup (() => OrderItemNomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
				.SelectSum (() => orderItemsAlias.Count).WithAlias (() => resultAlias.Amount)
				)
				.TransformUsing (Transformers.AliasToBean <GoodsInRouteListResult> ())
				.List<GoodsInRouteListResult> ();
		}

		public static IList<GoodsInRouteListResult> GetEquipmentsInRL (IUnitOfWork uow, RouteList routeList, Warehouse warehouse = null)
		{
			GoodsInRouteListResult resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null;
			Equipment equipmentAlias = null;

			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order> (() => orderAlias);

			var routeListItemsSubQuery = QueryOver.Of<RouteListItem> ()
				.Where (r => r.RouteList.Id == routeList.Id)
				.Select (r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty (o => o.Id).In (routeListItemsSubQuery).Select (o => o.Id);

			var orderitemsQuery = uow.Session.QueryOver<OrderItem>(() => orderItemsAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Where(() => !OrderItemNomenclatureAlias.Serial);
			if (warehouse != null)
				orderitemsQuery.Where(() => OrderItemNomenclatureAlias.Warehouse == warehouse);

			var orderEquipmentsQuery = uow.Session.QueryOver<OrderEquipment>(() => orderEquipmentAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => OrderEquipmentNomenclatureAlias);
			if (warehouse != null)
				orderEquipmentsQuery.Where(() => OrderEquipmentNomenclatureAlias.Warehouse == warehouse);
			
			return orderEquipmentsQuery
				.SelectList (list => list
					.SelectGroup (() => OrderEquipmentNomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.SelectGroup (() => equipmentAlias.Id).WithAlias (() => resultAlias.EquipmentId)
					.SelectSum (() => 1).WithAlias (() => resultAlias.Amount)
				)
				.TransformUsing (Transformers.AliasToBean <GoodsInRouteListResult> ())
				.List<GoodsInRouteListResult> ();
		}

		public static IList<GoodsLoadedListResult> AllGoodsLoaded(IUnitOfWork UoW, RouteList routeList, CarLoadDocument excludeDoc = null)
		{
			CarLoadDocument docAlias = null;
			CarLoadDocumentItem docItemsAlias = null;

			GoodsLoadedListResult inCarLoads = null;
			var loadedQuery = UoW.Session.QueryOver<CarLoadDocument>(() => docAlias)
				.Where(d => d.RouteList.Id == routeList.Id);
			if (excludeDoc != null)
				loadedQuery.Where(d => d.Id != excludeDoc.Id);
			
			var loadedlist = loadedQuery
				.JoinAlias(d => d.Items, () => docItemsAlias)
				.SelectList (list => list
					.SelectGroup (() => docItemsAlias.Nomenclature.Id).WithAlias (() => inCarLoads.NomenclatureId)
					.SelectGroup (() => docItemsAlias.Equipment.Id).WithAlias (() => inCarLoads.EquipmentId)
					.SelectSum (() => docItemsAlias.Amount).WithAlias (() => inCarLoads.Amount)
				).TransformUsing (Transformers.AliasToBean <GoodsLoadedListResult> ())
				.List<GoodsLoadedListResult> ();
			return loadedlist;			      
		}
			
		public class GoodsInRouteListResult{
			public int NomenclatureId { get; set;}
			public int EquipmentId { get; set;}
			public int Amount { get; set;}
		}

		public class GoodsLoadedListResult{
			public int NomenclatureId { get; set;}
			public int EquipmentId { get; set;}
			public decimal Amount { get; set;}
		}

	}
}

