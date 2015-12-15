using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Store;
using System.Data.Bindings;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Logistic;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Gamma.ColumnConfig;
using QSOrmProject;
using System.Linq;

namespace Vodovoz.ViewModel
{
	public class ReadyForReceptionVM : RepresentationModelWithoutEntityBase<ReadyForReceptionVMNode>
	{
		public ReadyForReceptionVM () : this (UnitOfWorkFactory.CreateWithoutRoot ())
		{
		}

		public ReadyForReceptionVM (IUnitOfWork uow) : base (typeof(RouteList), typeof(Vodovoz.Domain.Orders.Order))
		{
			this.UoW = uow;
		}

		public ReadyForReceptionFilter Filter {
			get {
				return RepresentationFilter as ReadyForReceptionFilter;
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
			ReadyForReceptionVMNode resultAlias = null;
			Employee employeeAlias = null;
			Car carAlias = null;

			List<ReadyForReceptionVMNode> items = new List<ReadyForReceptionVMNode> ();

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
				.Where (r => routeListAlias.Status == RouteListStatus.Ready);

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
						.Select (() => employeeAlias.Name).WithAlias (() => resultAlias.Name)
						.Select (() => employeeAlias.LastName).WithAlias (() => resultAlias.LastName)
						.Select (() => employeeAlias.Patronymic).WithAlias (() => resultAlias.Patronymic)
						.Select (() => carAlias.RegistrationNumber).WithAlias (() => resultAlias.Car)
					)
					.TransformUsing (Transformers.AliasToBean <ReadyForReceptionVMNode> ())
					.List<ReadyForReceptionVMNode> ());

			SetItemsSource (items);
		}

		IColumnsConfig columnsConfig = FluentColumnsConfig<ReadyForReceptionVMNode>.Create ()
			.AddColumn ("Маршрутный лист").SetDataProperty (node => node.Id)
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

	public class ReadyForReceptionVMNode{
		public int Id{ get; set; }

		public string Name { get; set; }

		public string LastName { get; set; }

		public string Patronymic { get; set; }

		public string Driver { get { return String.Format ("{0} {1} {2}", LastName, Name, Patronymic); } }

		public string Car { get; set; }
	}
}

