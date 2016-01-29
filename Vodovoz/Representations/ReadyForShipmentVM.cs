using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;

namespace Vodovoz.ViewModel
{
	public class ReadyForShipmentVM : RepresentationModelWithoutEntityBase<ReadyForShipmentVMNode>
	{
		public ReadyForShipmentVM () : this (UnitOfWorkFactory.CreateWithoutRoot ())
		{
		}

		public ReadyForShipmentVM (IUnitOfWork uow) : base (typeof(RouteList), typeof(Vodovoz.Domain.Orders.Order))
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

			if (Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == ShipmentDocumentType.RouteList) {

				var queryRoutes = UoW.Session.QueryOver<RouteList> (() => routeListAlias)
					.JoinAlias (rl => rl.Driver, () => employeeAlias)
					.JoinAlias (rl => rl.Car, () => carAlias)
					.Where (r => routeListAlias.Status == RouteListStatus.Ready 
						|| routeListAlias.Status == RouteListStatus.InLoading);

				if (Filter.RestrictWarehouse != null) {

					queryRoutes.JoinAlias (rl => rl.Addresses, () => routeListAddressAlias)
						.JoinAlias (() => routeListAddressAlias.Order, () => orderAlias)
						.Where (new Disjunction ()
							.Add (Subqueries.WhereExists (orderitemsSubqury))
							.Add (Subqueries.WhereExists (orderEquipmentSubquery))
						);
				}
				
				items.AddRange (
				queryRoutes.SelectList (list => list
						.Select (() => routeListAlias.Id).WithAlias (() => resultAlias.Id)
						.Select (() => ShipmentDocumentType.RouteList).WithAlias (() => resultAlias.TypeEnum)
						.Select (() => employeeAlias.Name).WithAlias (() => resultAlias.Name)
						.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.LastName)
						.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.Patronymic)
						.Select (() => carAlias.RegistrationNumber).WithAlias (() => resultAlias.Car)
					)
					.TransformUsing (Transformers.AliasToBean <ReadyForShipmentVMNode> ())
					.List<ReadyForShipmentVMNode> ());
			}

			if (Filter.RestrictDocumentType == null || Filter.RestrictDocumentType == ShipmentDocumentType.Order) {
				var queryOrders = UoW.Session.QueryOver<Vodovoz.Domain.Orders.Order> (() => orderAlias)
				.Where (() => orderAlias.SelfDelivery == true && orderAlias.OrderStatus == OrderStatus.Accepted);
				if (Filter.RestrictWarehouse != null) {
					
					queryOrders.Where (new Disjunction ()
							.Add (Subqueries.WhereExists (orderitemsSubqury))
							.Add (Subqueries.WhereExists (orderEquipmentSubquery))
					);
				}

				items.AddRange (
				queryOrders.SelectList (list => list
					.Select (() => orderAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => ShipmentDocumentType.Order).WithAlias (() => resultAlias.TypeEnum)
					.Select (() => "-").WithAlias (() => resultAlias.Name)
					.Select (() => "").WithAlias (() => resultAlias.LastName)
					.Select (() => "").WithAlias (() => resultAlias.Patronymic)
					.Select (() => "-").WithAlias (() => resultAlias.Car)
				)
				.TransformUsing (Transformers.AliasToBean<ReadyForShipmentVMNode> ())
				.List<ReadyForShipmentVMNode> ().ToList ()
				);
			}

			SetItemsSource (items);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ReadyForShipmentVMNode>.Create ()
			.AddColumn ("Тип").SetDataProperty (node => node.TypeString)
			.AddColumn ("Номер").SetDataProperty (node => node.Id)
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
		public int Id{ get; set; }

		public ShipmentDocumentType TypeEnum { get; set; }

		public string TypeString { get { return TypeEnum.GetEnumTitle (); } }

		public string Name { get; set; }

		public string LastName { get; set; }

		public string Patronymic { get; set; }

		public string Driver { get { return String.Format ("{0} {1} {2}", LastName, Name, Patronymic); } }

		public string Car { get; set; }
	}

}

