using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Goods;
using QSSupportLib;
using QSProjectsLib;

namespace Vodovoz.Repository.Store
{
	public static class WarehouseRepository
	{
		const string defaultWarehouseForProduction = "production_warehouse";
		const string defaultWaterWarehouse = "water_warehouse";
		const string defaultOfficeWarehouse = "office_warehouse";
		const string defaultEquipmenteWarehouse = "equipment_warehouse";

		public static IList<Warehouse> GetActiveWarehouse(IUnitOfWork uow)
		{
			return uow.Session.CreateCriteria<Warehouse>().List<Warehouse>();
		}

		public static QueryOver<Warehouse> ActiveWarehouseQuery()
		{
			return QueryOver.Of<Warehouse>();
		}

		public static IList<Warehouse> WarehouseForShipment(IUnitOfWork uow, int routeListId)
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null, resultNomenclatureAlias = null;

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
				.JoinQueryOver(() => orderEquipmentAlias.Equipment)
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
				.JoinAlias(() => orderNewEquipmentAlias.NewEquipmentNomenclature, () => OrderNewEquipmentNomenclatureAlias)
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


		public static Warehouse DefaultWarehouseForProduction(IUnitOfWork uow)
		{
			int id = -1;
			if (MainSupport.BaseParameters.All.ContainsKey(defaultWarehouseForProduction) &&
				int.TryParse(MainSupport.BaseParameters.All[defaultWarehouseForProduction], out id))
				return uow.Session.QueryOver<Warehouse>()
					.Where(fExp => fExp.Id == id)
					.Take(1)
					.SingleOrDefault();

			throw new Exception(String.Format("Не создан параметр={0} в base_parameters", defaultWarehouseForProduction));
		}

		public static Warehouse DefaultWarehouseForWater(IUnitOfWork uow)
		{
			int id = -1;
			if (MainSupport.BaseParameters.All.ContainsKey(defaultWaterWarehouse) &&
				int.TryParse(MainSupport.BaseParameters.All[defaultWaterWarehouse], out id))
				return uow.Session.QueryOver<Warehouse>()
					.Where(fExp => fExp.Id == id)
					.Take(1)
					.SingleOrDefault();

			throw new Exception(String.Format("Не создан параметр={0} в base_parameters", defaultWaterWarehouse));
		}

		public static Warehouse DefaultWarehouseForOffice(IUnitOfWork uow)
		{
			int id = -1;
			if (MainSupport.BaseParameters.All.ContainsKey(defaultOfficeWarehouse) &&
				int.TryParse(MainSupport.BaseParameters.All[defaultOfficeWarehouse], out id))
				return uow.Session.QueryOver<Warehouse>()
					.Where(fExp => fExp.Id == id)
					.Take(1)
					.SingleOrDefault();

			throw new Exception(String.Format("Не создан параметр={0} в base_parameters", defaultOfficeWarehouse));
		}

		public static Warehouse DefaultWarehouseForEquipment(IUnitOfWork uow)
		{
			int id = -1;
			if (MainSupport.BaseParameters.All.ContainsKey(defaultEquipmenteWarehouse) &&
				int.TryParse(MainSupport.BaseParameters.All[defaultEquipmenteWarehouse], out id))
				return uow.Session.QueryOver<Warehouse>()
					.Where(fExp => fExp.Id == id)
					.Take(1)
					.SingleOrDefault();

			throw new Exception(String.Format("Не создан параметр={0} в base_parameters", defaultEquipmenteWarehouse));
		}

		public static Warehouse WarehouseByPermission(IUnitOfWork uow)
		{
			if (QSMain.User.Permissions["store_production"])
			{
				return WarehouseRepository.DefaultWarehouseForProduction(uow);
			}
			if (QSMain.User.Permissions["store_office"])
			{
				return WarehouseRepository.DefaultWarehouseForOffice(uow);
			}
			if (QSMain.User.Permissions["store_equipment"])
			{
				return WarehouseRepository.DefaultWarehouseForEquipment(uow);
			}
			return null;
		}
	}
}