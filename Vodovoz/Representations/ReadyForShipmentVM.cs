using System;
using System.Collections.Generic;
using Gamma.ColumnConfig;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Goods;

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

			List<ReadyForShipmentVMNode> items = new List<ReadyForShipmentVMNode> ();

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

			var alreadyLoadedRouteListsSubquery = QueryOver.Of<CarLoadDocument>(() => carLoadDocAlias)
				.Where(() => carLoadDocAlias.RouteList.Id == routeListAlias.Id)
				.Where(() => carLoadDocAlias.Warehouse == Filter.RestrictWarehouse)
				.Select(doc => doc.Id);

			var queryRoutes = UoW.Session.QueryOver<RouteList> (() => routeListAlias)
				.JoinAlias (rl => rl.Driver, () => employeeAlias)
				.JoinAlias (rl => rl.Car, () => carAlias)
				.Where (r => routeListAlias.Status == RouteListStatus.InLoading 
					|| routeListAlias.Status == RouteListStatus.InLoading);

			if (Filter.RestrictWarehouse != null) {

				queryRoutes.JoinAlias (rl => rl.Addresses, () => routeListAddressAlias)
					.JoinAlias (() => routeListAddressAlias.Order, () => orderAlias)
					.Where (new Disjunction ()
						.Add (Subqueries.WhereExists (orderitemsSubqury))
						.Add (Subqueries.WhereExists (orderEquipmentSubquery))
					)
					.Where(Subqueries.WhereNotExists (alreadyLoadedRouteListsSubquery));
			}
			
			items.AddRange (
			queryRoutes.SelectList (list => list
					.SelectGroup (() => routeListAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => employeeAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.LastName)
					.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.Patronymic)
					.Select (() => carAlias.RegistrationNumber).WithAlias (() => resultAlias.Car)
				)
				.TransformUsing (Transformers.AliasToBean <ReadyForShipmentVMNode> ())
				.List<ReadyForShipmentVMNode> ());

			SetItemsSource (items);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ReadyForShipmentVMNode>.Create ()
			.AddColumn ("Тип").SetDataProperty (node => node.TypeString)
			.AddColumn ("Номер").AddTextRenderer (node => node.Id.ToString())
		    .AddColumn ("Водитель").SetDataProperty (node => node.Driver)
		    .AddColumn ("Машина").SetDataProperty (node => node.Car)
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
	}

}

