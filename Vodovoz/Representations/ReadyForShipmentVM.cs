using System;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Logistic;
using QSOrmProject;
using Vodovoz.Domain.Orders;
using Gtk.DataBindings;
using System.Data.Bindings;
using NHibernate.Transform;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.ViewModel
{
	public class ReadyForShipmentVM : RepresentationModelBase<RouteList, ReadyForShipmentVMNode>
	{
		public ReadyForShipmentVM () : this (UnitOfWorkFactory.CreateWithoutRoot ())
		{
		}

		public ReadyForShipmentVM (IUnitOfWork uow) : base (typeof(RouteList), typeof(Order))
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
			Order orderAlias = null;
			RouteList routeListAlias = null;
			ReadyForShipmentVMNode resultAlias = null;

			List<ReadyForShipmentVMNode> items = new List<ReadyForShipmentVMNode> ();

			if (Filter == null || Filter.RestrictDocumentType != ShipmentDocumentType.RouteList) {
				items.AddRange (UoW.Session.QueryOver<Order> (() => orderAlias)
				.Where (() => orderAlias.SelfDelivery == true && orderAlias.OrderStatus == OrderStatus.Accepted)
				.SelectList (list => list
					.Select (() => orderAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => ShipmentDocumentType.Order).WithAlias (() => resultAlias.TypeEnum)
				)
				.TransformUsing (Transformers.AliasToBean<ReadyForShipmentVMNode> ())
				.List<ReadyForShipmentVMNode> ().ToList ());
			}

			if (Filter == null || Filter.RestrictDocumentType != ShipmentDocumentType.Order) {
				items.AddRange (UoW.Session.QueryOver<RouteList> (() => routeListAlias)
				.Where (r => routeListAlias.Status == RouteListStatus.Ready)
				.SelectList (list => list
					.Select (() => routeListAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => ShipmentDocumentType.RouteList).WithAlias (() => resultAlias.TypeEnum)
				)
					.TransformUsing (Transformers.AliasToBean <ReadyForShipmentVMNode> ())
				.List<ReadyForShipmentVMNode> ());
			}
			SetItemsSource (items);
		}

		IMappingConfig treeViewConfig = FluentMappingConfig<ReadyForShipmentVMNode>.Create ()
			.AddColumn ("Тип").SetDataProperty (node => node.TypeString)
			.AddColumn ("Номер").SetDataProperty (node => node.Id)
		                                //.AddColumn ("Водитель").SetDataProperty (node => node.Driver)
		                                //.AddColumn ("Машина").SetDataProperty (node => node.Car)
			.Finish ();

		public override IMappingConfig TreeViewConfig {
			get { return treeViewConfig; }
		}

		protected override bool NeedUpdateFunc (RouteList updatedSubject)
		{
			throw new NotImplementedException ();
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

		//public string Driver { get; set; }

		//public string Car { get; set; }
	}

	public enum ShipmentDocumentType
	{
		[ItemTitleAttribute ("Заказ")]
		Order,
		[ItemTitleAttribute ("Маршрутный лист")]
		RouteList
	}
}

