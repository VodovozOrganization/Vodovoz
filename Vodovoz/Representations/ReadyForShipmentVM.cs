using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ViewModel
{
	public class ReadyForShipmentVM : RepresentationModelWithoutEntityBase<ReadyForShipmentVMNode>
	{
		public ReadyForShipmentVM () : this (UnitOfWorkFactory.CreateWithoutRoot ())
		{
		}

		public ReadyForShipmentVM (IUnitOfWork uow) : base (
			typeof(RouteList),
			typeof(Vodovoz.Domain.Orders.Order),
			typeof(CarLoadDocument)
		)
		{
			this.UoW = uow;
		}

		public ReadyForShipmentFilter Filter {
			get {
				return RepresentationFilter as ReadyForShipmentFilter;
			}
			set {
				RepresentationFilter = value as IRepresentationFilter;
			}
		}

		#region implemented abstract members of RepresentationModelBase

		public override void UpdateNodes ()
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Equipment equipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null;

			RouteList routeListAlias = null;
			RouteListItem routeListAddressAlias = null;
			ReadyForShipmentVMNode resultAlias = null;
			Employee employeeAlias = null;
			Car carAlias = null;
			CarLoadDocument carLoadDocAlias = null;

			var orderitemsSubqury = QueryOver.Of<OrderItem> (() => orderItemsAlias)
				.Where (() => orderItemsAlias.Order.Id == orderAlias.Id)
				.JoinAlias (() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.Where (() => OrderItemNomenclatureAlias.Warehouse == Filter.RestrictWarehouse)
				.Select (i => i.Order);
			var orderEquipmentSubquery = QueryOver.Of<OrderEquipment> (() => orderEquipmentAlias)
				.Where(() => orderEquipmentAlias.Order.Id == orderAlias.Id)
				.JoinAlias (() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.JoinAlias (() => equipmentAlias.Nomenclature, () => OrderEquipmentNomenclatureAlias)
				.Where(() => OrderEquipmentNomenclatureAlias.Warehouse == Filter.RestrictWarehouse && orderEquipmentAlias.Direction == Direction.Deliver)
				.Select (i => i.Order);

			var queryRoutes = UoW.Session.QueryOver<RouteList> (() => routeListAlias)
				.JoinAlias (rl => rl.Driver, () => employeeAlias)
				.JoinAlias (rl => rl.Car, () => carAlias)
				.Where (r => routeListAlias.Status == RouteListStatus.InLoading);

			if (Filter.RestrictWarehouse != null) {

				queryRoutes.JoinAlias (rl => rl.Addresses, () => routeListAddressAlias)
					.JoinAlias (() => routeListAddressAlias.Order, () => orderAlias)
					.Where(() => !routeListAddressAlias.WasTransfered || (routeListAddressAlias.WasTransfered && routeListAddressAlias.NeedToReload))
					.Where (new Disjunction ()
						.Add (Subqueries.WhereExists (orderitemsSubqury))
						.Add (Subqueries.WhereExists (orderEquipmentSubquery))
					);
			}
			
			var dirtyList =	queryRoutes.SelectList (list => list
					.SelectGroup (() => routeListAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => employeeAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.LastName)
					.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.Patronymic)
					.Select (() => carAlias.RegistrationNumber).WithAlias (() => resultAlias.Car)
					.Select(() => routeListAlias.Date).WithAlias(() => resultAlias.Date)
				)
				.TransformUsing (Transformers.AliasToBean <ReadyForShipmentVMNode> ())
				.List<ReadyForShipmentVMNode> ();

			if(Filter.RestrictWarehouse != null)
			{
				List<ReadyForShipmentVMNode> resultList = new List<ReadyForShipmentVMNode> ();
				var routes = UoW.GetById<RouteList>(dirtyList.Select(x => x.Id));
				foreach(var dirty in dirtyList)
				{
					var route = routes.First(x => x.Id == dirty.Id);
					var inLoaded = Repository.Logistics.RouteListRepository.AllGoodsLoaded(UoW, route);
					var goods = Repository.Logistics.RouteListRepository.GetGoodsInRLWithoutEquipments(UoW, route, Filter.RestrictWarehouse);

					bool closed = true;
					foreach(var good in goods)
					{
						var loaded = inLoaded.FirstOrDefault(x => x.NomenclatureId == good.NomenclatureId);
						if(loaded == null || loaded.Amount < good.Amount)
						{
							closed = false;
							break;
						}
					}
					if(closed == true)
					{
						var equipmentsInRoute = Repository.Logistics.RouteListRepository.GetEquipmentsInRL(UoW, route, Filter.RestrictWarehouse);
						foreach(var equipment in equipmentsInRoute)
						{
							var loaded = inLoaded.FirstOrDefault(x => x.EquipmentId == equipment.EquipmentId);
							if(loaded == null || loaded.Amount < equipment.Amount)
							{
								closed = false;
								break;
							}
						}
					}
					if (!closed)
						resultList.Add(dirty);
				}

				SetItemsSource (resultList.OrderByDescending(x => x.Date).ToList());
			}
			else
				SetItemsSource (dirtyList.OrderByDescending(x => x.Date).ToList());
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ReadyForShipmentVMNode>.Create ()
			.AddColumn ("Тип").SetDataProperty (node => node.TypeString)
			.AddColumn ("Номер").AddTextRenderer (node => node.Id.ToString())
		    .AddColumn ("Водитель").SetDataProperty (node => node.Driver)
		    .AddColumn ("Машина").SetDataProperty (node => node.Car)
			.AddColumn ("Дата").AddTextRenderer (node => node.Date.ToShortDateString())
			.Finish ();

		public override IColumnsConfig ColumnsConfig {
			get { return columnsConfig; }
		}

		protected override bool NeedUpdateFunc (object updatedSubject)
		{
			return true;
		}

		#endregion
	}

	public class ReadyForShipmentVMNode
	{
		[UseForSearch]
		[SearchHighlight]
		public int Id{ get; set; }

		public string TypeString { get { return "Маршрутный лист"; } }

		public string Name { get; set; }

		public string LastName { get; set; }

		public string Patronymic { get; set; }

		[UseForSearch]
		[SearchHighlight]
		public string Driver { get { return String.Format ("{0} {1} {2}", LastName, Name, Patronymic); } }

		[UseForSearch]
		[SearchHighlight]
		public string Car { get; set; }

		public DateTime Date { get;	set; }
	}

}

