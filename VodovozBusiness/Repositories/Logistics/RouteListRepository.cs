using System;
using System.Collections.Generic;
using System.Linq;
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

		public static IList<GoodsInRouteListResult> GetGoodsAndEquipsInRL(IUnitOfWork uow, RouteList routeList, Warehouse warehouse = null)
		{
			List<GoodsInRouteListResult> result = new List<GoodsInRouteListResult>();

			result.AddRange(GetGoodsInRLWithoutEquipments(uow, routeList, warehouse).ToList());
			result.AddRange(GetEquipmentsInRL(uow, routeList, warehouse).ToList());

			return result;
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

			var orderitemsQuery = uow.Session.QueryOver<OrderItem>(() => orderItemsAlias)
				.WithSubquery.WhereProperty(i => i.Order.Id).In(ordersQuery)
				.JoinAlias(() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Where(() => OrderItemNomenclatureAlias.Category.IsIn(Nomenclature.GetCategoriesForShipment()))
				.Where(() => OrderItemNomenclatureAlias.Category != NomenclatureCategory.equipment);
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
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature OrderEquipmentNomenclatureAlias = null;

			//Выбирается список Id заказов находящихся в МЛ
			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order> (() => orderAlias);
			var routeListItemsSubQuery = QueryOver.Of<RouteListItem> ()
				.Where (r => r.RouteList.Id == routeList.Id)
				.Select (r => r.Order.Id);
			ordersQuery.WithSubquery.WhereProperty (o => o.Id).In (routeListItemsSubQuery).Select (o => o.Id);

			var orderEquipmentsQuery = uow.Session.QueryOver<OrderEquipment> (() => orderEquipmentAlias)
				.WithSubquery.WhereProperty (i => i.Order.Id).In (ordersQuery)
			    .Where (() => orderEquipmentAlias.Direction == Direction.Deliver)
			    .JoinAlias (() => orderEquipmentAlias.Nomenclature, () => OrderEquipmentNomenclatureAlias);
			if (warehouse != null)
				orderEquipmentsQuery.Where (() => OrderEquipmentNomenclatureAlias.Warehouse == warehouse);

			return orderEquipmentsQuery
				.SelectList (list => list
					.SelectGroup (() => OrderEquipmentNomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
				             .SelectSum (() => orderEquipmentAlias.Count).WithAlias (() => resultAlias.Amount)
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
					.SelectSum (() => docItemsAlias.Amount).WithAlias (() => inCarLoads.Amount)
				).TransformUsing (Transformers.AliasToBean<GoodsLoadedListResult> ())
				.List<GoodsLoadedListResult> ();
			return loadedlist;
		}

		public static List<ReturnsNode> GetReturnsToWarehouse (IUnitOfWork uow, int routeListId, NomenclatureCategory [] categories, int[] excludeNomenclatureIds = null)
		{
			List<ReturnsNode> result = new List<ReturnsNode> ();
			Nomenclature nomenclatureAlias = null;
			ReturnsNode resultAlias = null;
			Equipment equipmentAlias = null;
			CarUnloadDocumentItem carUnloadItemsAlias = null;
			WarehouseMovementOperation movementOperationAlias = null;

			var returnableQuery = uow.Session.QueryOver<CarUnloadDocument> ().Where (doc => doc.RouteList.Id == routeListId)
				.JoinAlias (doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias (() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where (Restrictions.IsNotNull (Projections.Property (() => movementOperationAlias.IncomingWarehouse)))
				.JoinAlias (() => movementOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where (() => !nomenclatureAlias.IsSerial)
				.Where (() => nomenclatureAlias.Category.IsIn (categories));
			if (excludeNomenclatureIds != null)
				returnableQuery.Where (() => !nomenclatureAlias.Id.IsIn (excludeNomenclatureIds));

			var returnableItems =
				returnableQuery.SelectList (list => list
					 .SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					 .Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					 .Select (() => false).WithAlias (() => resultAlias.Trackable)
					 .Select (() => nomenclatureAlias.Category).WithAlias (() => resultAlias.NomenclatureCategory)
					 .SelectSum (() => movementOperationAlias.Amount).WithAlias (() => resultAlias.Amount)
								  )
				.TransformUsing (Transformers.AliasToBean<ReturnsNode> ())
				.List<ReturnsNode> ();
			
			var returnableQueryEquipment = uow.Session.QueryOver<CarUnloadDocument> ().Where (doc => doc.RouteList.Id == routeListId)
				.JoinAlias (doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias (() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where (Restrictions.IsNotNull (Projections.Property (() => movementOperationAlias.IncomingWarehouse)))
				.JoinAlias (() => movementOperationAlias.Equipment, () => equipmentAlias)
				.JoinAlias (() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where (() => nomenclatureAlias.Category.IsIn (categories));
			if (excludeNomenclatureIds != null)
				returnableQueryEquipment.Where (() => !nomenclatureAlias.Id.IsIn (excludeNomenclatureIds));

			var returnableEquipment =
				returnableQueryEquipment.SelectList (list => list
					 .Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.Id)
					 .SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					 .Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					 .Select (() => nomenclatureAlias.IsSerial).WithAlias (() => resultAlias.Trackable)
					 .Select (() => nomenclatureAlias.Category).WithAlias (() => resultAlias.NomenclatureCategory)
					 .SelectSum (() => movementOperationAlias.Amount).WithAlias (() => resultAlias.Amount)
					 .Select (() => nomenclatureAlias.Type).WithAlias (() => resultAlias.EquipmentType)
									  )
				.TransformUsing (Transformers.AliasToBean<ReturnsNode> ())
				.List<ReturnsNode> ();

			result.AddRange (returnableItems);
			result.AddRange (returnableEquipment);
			DomainHelper.FillPropertyByEntity<ReturnsNode, Nomenclature>(uow, result, x => x.NomenclatureId, (node, nom) => node.Nomenclature = nom);
			return result;
		}

		/// <summary>
		/// Возвращает список товаров возвращенного на склад только по 1 номенклатуре
		/// </summary>
		public static List<ReturnsNode> GetReturnsToWarehouse(IUnitOfWork uow, int routeListId, int nomenclatureId)
		{
			List<ReturnsNode> result = new List<ReturnsNode>();
			Nomenclature nomenclatureAlias = null;
			ReturnsNode resultAlias = null;
			Equipment equipmentAlias = null;
			CarUnloadDocumentItem carUnloadItemsAlias = null;
			WarehouseMovementOperation movementOperationAlias = null;

			var returnableQuery = uow.Session.QueryOver<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeListId)
				.JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where(Restrictions.IsNotNull(Projections.Property(() => movementOperationAlias.IncomingWarehouse)))
				.JoinAlias(() => movementOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => !nomenclatureAlias.IsSerial)
			    .Where(() => nomenclatureAlias.Id == nomenclatureId);
			

			var returnableItems =
				returnableQuery.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => false).WithAlias(() => resultAlias.Trackable)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					.SelectSum(() => movementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
								  )
				.TransformUsing(Transformers.AliasToBean<ReturnsNode>())
				.List<ReturnsNode>();

			var returnableQueryEquipment = uow.Session.QueryOver<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeListId)
				.JoinAlias(doc => doc.Items, () => carUnloadItemsAlias)
				.JoinAlias(() => carUnloadItemsAlias.MovementOperation, () => movementOperationAlias)
				.Where(Restrictions.IsNotNull(Projections.Property(() => movementOperationAlias.IncomingWarehouse)))
				.JoinAlias(() => movementOperationAlias.Equipment, () => equipmentAlias)
				.JoinAlias(() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Id == nomenclatureId);

			var returnableEquipment =
				returnableQueryEquipment.SelectList(list => list
					.Select(() => equipmentAlias.Id).WithAlias(() => resultAlias.Id)
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => nomenclatureAlias.IsSerial).WithAlias(() => resultAlias.Trackable)
					.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.NomenclatureCategory)
					.SelectSum(() => movementOperationAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(() => nomenclatureAlias.Type).WithAlias(() => resultAlias.EquipmentType)
									  )
				.TransformUsing(Transformers.AliasToBean<ReturnsNode>())
				.List<ReturnsNode>();

			result.AddRange(returnableItems);
			result.AddRange(returnableEquipment);
			DomainHelper.FillPropertyByEntity<ReturnsNode, Nomenclature>(uow, result, x => x.NomenclatureId, (node, nom) => node.Nomenclature = nom);
			return result;
		}


		#region DTO

		public class ReturnsNode
		{
			public int Id { get; set; }
			public Nomenclature Nomenclature { get; set; }
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
			public int Amount { get; set; }
		}

		public class GoodsLoadedListResult
		{
			public int NomenclatureId { get; set; }
			public decimal Amount { get; set; }
		}

		#endregion
	}
}

