using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;

namespace Vodovoz.Repository.Logistics
{
	public static class RouteListRepository
	{
		public static IList<RouteList> GetDriverRouteLists (IUnitOfWork uow, Employee driver, RouteListStatus status, DateTime date)
		{
			RouteList routeListAlias = null;

			return uow.Session.QueryOver<RouteList> (() => routeListAlias)
					  .Where (() => routeListAlias.Driver == driver)
				      .Where (() => routeListAlias.Status == status)
				      .Where (() => routeListAlias.Date == date)
					  .List ();
		}

		public static QueryOver<RouteList> GetRoutesAtDay (DateTime date)
		{
			return QueryOver.Of<RouteList> ()
				.Where (x => x.Date == date);
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
				.Where (r => !r.WasTransfered || (r.WasTransfered && r.NeedToReload))
				.Select (r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty (o => o.Id).In (routeListItemsSubQuery).Select (o => o.Id);

			var orderitemsQuery = uow.Session.QueryOver<OrderItem> (() => orderItemsAlias)
				.WithSubquery.WhereProperty (i => i.Order.Id).In (ordersQuery)
				.JoinAlias (() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Where (() => !OrderItemNomenclatureAlias.Serial)
				.Where (() => OrderItemNomenclatureAlias.Category.IsIn (Nomenclature.GetCategoriesForShipment ()));
			if (warehouse != null)
				orderitemsQuery.Where (() => OrderItemNomenclatureAlias.Warehouse == warehouse);

			return orderitemsQuery.SelectList (list => list
				.SelectGroup (() => OrderItemNomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
				.SelectSum (() => orderItemsAlias.Count).WithAlias (() => resultAlias.Amount)
				)
				.TransformUsing (Transformers.AliasToBean<GoodsInRouteListResult> ())
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

			var orderitemsQuery = uow.Session.QueryOver<OrderItem> (() => orderItemsAlias)
				.WithSubquery.WhereProperty (i => i.Order.Id).In (ordersQuery)
				.JoinAlias (() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Where (() => !OrderItemNomenclatureAlias.Serial);
			if (warehouse != null)
				orderitemsQuery.Where (() => OrderItemNomenclatureAlias.Warehouse == warehouse);

			var orderEquipmentsQuery = uow.Session.QueryOver<OrderEquipment> (() => orderEquipmentAlias)
				.WithSubquery.WhereProperty (i => i.Order.Id).In (ordersQuery)
				.JoinAlias (() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.Where (() => orderEquipmentAlias.Direction == Direction.Deliver)
				.JoinAlias (() => equipmentAlias.Nomenclature, () => OrderEquipmentNomenclatureAlias);
			if (warehouse != null)
				orderEquipmentsQuery.Where (() => OrderEquipmentNomenclatureAlias.Warehouse == warehouse);

			return orderEquipmentsQuery
				.SelectList (list => list
					.SelectGroup (() => OrderEquipmentNomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.SelectGroup (() => equipmentAlias.Id).WithAlias (() => resultAlias.EquipmentId)
					.SelectSum (() => 1).WithAlias (() => resultAlias.Amount)
				)
				.TransformUsing (Transformers.AliasToBean<GoodsInRouteListResult> ())
				.List<GoodsInRouteListResult> ();
		}

		public static IList<GoodsLoadedListResult> AllGoodsLoaded (IUnitOfWork UoW, RouteList routeList, CarLoadDocument excludeDoc = null)
		{
			CarLoadDocument docAlias = null;
			CarLoadDocumentItem docItemsAlias = null;

			GoodsLoadedListResult inCarLoads = null;
			var loadedQuery = UoW.Session.QueryOver<CarLoadDocument> (() => docAlias)
				.Where (d => d.RouteList.Id == routeList.Id);
			if (excludeDoc != null)
				loadedQuery.Where (d => d.Id != excludeDoc.Id);

			var loadedlist = loadedQuery
				.JoinAlias (d => d.Items, () => docItemsAlias)
				.SelectList (list => list
					.SelectGroup (() => docItemsAlias.Nomenclature.Id).WithAlias (() => inCarLoads.NomenclatureId)
					.SelectGroup (() => docItemsAlias.Equipment.Id).WithAlias (() => inCarLoads.EquipmentId)
					.SelectSum (() => docItemsAlias.Amount).WithAlias (() => inCarLoads.Amount)
				).TransformUsing (Transformers.AliasToBean<GoodsLoadedListResult> ())
				.List<GoodsLoadedListResult> ();
			return loadedlist;
		}

		public static List<ReturnsNode> GetReturnsToWarehouse (IUnitOfWork uow, int routeListId, NomenclatureCategory [] categories)
		{
			List<ReturnsNode> result = new List<ReturnsNode> ();
			Nomenclature nomenclatureAlias = null;
			ReturnsNode resultAlias = null;
			Equipment equipmentAlias = null;
			CarUnloadDocumentItem carUnloadItemsAlias = null;
			WarehouseMovementOperation movementOperationAlias = null;

			var returnableItems = uow.Session.QueryOver<CarUnloadDocument> ().Where (doc => doc.RouteList.Id == routeListId)
				.JoinAlias (doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias (() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where (Restrictions.IsNotNull (Projections.Property (() => movementOperationAlias.IncomingWarehouse)))
				.JoinAlias (() => movementOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where (() => !nomenclatureAlias.Serial)
				.Where (() => nomenclatureAlias.Category.IsIn (categories))
				.SelectList (list => list
					 .SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					 .Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					 .Select (() => false).WithAlias (() => resultAlias.Trackable)
					 .Select (() => nomenclatureAlias.Category).WithAlias (() => resultAlias.NomenclatureCategory)
					 .SelectSum (() => movementOperationAlias.Amount).WithAlias (() => resultAlias.Amount)
								  )
				.TransformUsing (Transformers.AliasToBean<ReturnsNode> ())
				.List<ReturnsNode> ();

			var returnableEquipment = uow.Session.QueryOver<CarUnloadDocument> ().Where (doc => doc.RouteList.Id == routeListId)
				.JoinAlias (doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias (() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where (Restrictions.IsNotNull (Projections.Property (() => movementOperationAlias.IncomingWarehouse)))
				.JoinAlias (() => movementOperationAlias.Equipment, () => equipmentAlias)
				.JoinAlias (() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where (() => nomenclatureAlias.Category.IsIn (categories))
				.SelectList (list => list
					 .Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.Id)
					 .SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					 .Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					 .Select (() => nomenclatureAlias.Serial).WithAlias (() => resultAlias.Trackable)
					 .Select (() => nomenclatureAlias.Category).WithAlias (() => resultAlias.NomenclatureCategory)
					 .SelectSum (() => movementOperationAlias.Amount).WithAlias (() => resultAlias.Amount)
					 .Select (() => nomenclatureAlias.Type).WithAlias (() => resultAlias.EquipmentType)
									  )
				.TransformUsing (Transformers.AliasToBean<ReturnsNode> ())
				.List<ReturnsNode> ();

			result.AddRange (returnableItems);
			result.AddRange (returnableEquipment);
			return result;
		}

		#region DTO

		public class ReturnsNode
		{
			public int Id { get; set; }
			public NomenclatureCategory NomenclatureCategory { get; set; }
			public int NomenclatureId { get; set; }
			public string Name { get; set; }
			public decimal Amount { get; set; }
			public bool Trackable { get; set; }
			public EquipmentType EquipmentType { get; set; }
			public string Serial {
				get {
					if (Trackable) {
						return Id > 0 ? Id.ToString () : "(не определен)";
					} else
						return String.Empty;
				}
			}
			public bool Returned {
				get {
					return Amount > 0;
				}
				set {
					Amount = value ? 1 : 0;
				}
			}
		}

		public class GoodsInRouteListResult
		{
			public int NomenclatureId { get; set; }
			public int EquipmentId { get; set; }
			public int Amount { get; set; }
		}

		public class GoodsLoadedListResult
		{
			public int NomenclatureId { get; set; }
			public int EquipmentId { get; set; }
			public decimal Amount { get; set; }
		}

		#endregion
	}
}

