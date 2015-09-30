using System;
using QSTDI;
using QSOrmProject;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using NHibernate.Criterion;
using System.Collections.Generic;
using NHibernate.Transform;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ReadyForShipmentDlg : TdiTabBase
	{
		private IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot ();

		ShipmentDocumentType shipmentType;
		int shipmentId;

		List<ShipmentItemsNode> ShipmentList = new List<ShipmentItemsNode>();

		public ReadyForShipmentDlg (ShipmentDocumentType type, int id, Warehouse stock)
		{
			Build ();
			shipmentType = type;
			shipmentId = id;

			this.TabName = "Товар на погрузку";
			ycomboboxWarehouse.ItemsList = Repository.Store.WarehouseRepository.WarehouseForShipment (UoW, type, id);

			ytreeItems.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ShipmentItemsNode> ()
				.AddColumn ("Номенклатуры").AddTextRenderer (node => node.NomenclatureName)
				.AddColumn ("Серийный номер").AddTextRenderer (node => node.SerialNumberText)
				.AddColumn ("Количество").AddTextRenderer (node => node.AmountText)
				.Finish ();

			if (stock != null)
				ycomboboxWarehouse.SelectedItem = stock;

			switch(shipmentType)
			{
			case ShipmentDocumentType.Order:
				var order = UoW.GetById<Vodovoz.Domain.Orders.Order> (id);
				textviewShipmentInfo.Buffer.Text =
					String.Format ("Самовывоз заказа №{0}\nКлиент: {1}",
					id,
					order.Client.FullName
				);
				TabName = String.Format ("Отгрузка заказа №{0}", id);
				break;
			case ShipmentDocumentType.RouteList:
				var routelist = UoW.GetById<RouteList> (id);
				textviewShipmentInfo.Buffer.Text =
					String.Format ("Маршрутный лист №{0} от {1:d}\nВодитель: {2}\nМашина: {3}({4})\nЭкспедитор: {5}",
						id,
						routelist.Date,
						routelist.Driver.FullName,
						routelist.Car.Model,
						routelist.Car.RegistrationNumber,
						routelist.Forwarder.FullName
					);
				TabName = String.Format ("Отгрузка маршрутного листа №{0}", id);
				break;
			}
		}

		void UpdateItemsList()
		{
			ShipmentList.Clear ();

			Warehouse CurrentStock = ycomboboxWarehouse.SelectedItem as Warehouse;
			if (CurrentStock == null)
				return;

			ShipmentItemsNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature OrderItemNomenclatureAlias = null, OrderEquipmentNomenclatureAlias = null;
			Equipment equipmentAlias = null;
			MeasurementUnits unitsAlias = null;

			RouteListItem routeListAddressAlias = null;

			var ordersQuery = QueryOver.Of<Vodovoz.Domain.Orders.Order> (() => orderAlias);

			switch (shipmentType) {
			case ShipmentDocumentType.Order:
				ordersQuery.Where (o => o.Id == shipmentId)
					.Select (o => o.Id);
				break;
			case ShipmentDocumentType.RouteList:
				var routeListItemsSubQuery = QueryOver.Of<Vodovoz.Domain.Logistic.RouteListItem> ()
					.Where (r => r.RouteList.Id == shipmentId)
					.Select (r => r.Order.Id);
				ordersQuery.WithSubquery.WhereProperty (o => o.Id).In (routeListItemsSubQuery).Select(o=>o.Id);
				break;
			default:
				throw new NotSupportedException (shipmentType.ToString ());
			}

			var orderitems = UoW.Session.QueryOver<OrderItem> (() => orderItemsAlias)
				.WithSubquery.WhereProperty (i => i.Order.Id).In (ordersQuery)
				.JoinAlias (() => orderItemsAlias.Nomenclature, () => OrderItemNomenclatureAlias)
				.JoinAlias (() => orderItemsAlias.Equipment, () => equipmentAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias (() => OrderItemNomenclatureAlias.Unit, () => unitsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where (() => OrderItemNomenclatureAlias.Warehouse == CurrentStock)
				.SelectList (list => list
					.SelectGroup (() => OrderItemNomenclatureAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => OrderItemNomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.SelectGroup (() => equipmentAlias.Id).WithAlias (() => resultAlias.EquipmentId)
					.SelectSum (() => orderItemsAlias.Count).WithAlias (() => resultAlias.Amount)
					.Select (() => unitsAlias.Name).WithAlias (() => resultAlias.UnitName)
				)
				.TransformUsing (Transformers.AliasToBean <ShipmentItemsNode> ())
				.List<ShipmentItemsNode> ();

			ShipmentList.AddRange (orderitems);
			ShipmentList.FindAll (node => node.EquipmentId > 0).ForEach (node => node.IsNew = true);

			var orderEquipments = UoW.Session.QueryOver<OrderEquipment> (() => orderEquipmentAlias)
				.WithSubquery.WhereProperty (i => i.Order.Id).In (ordersQuery)
				.JoinAlias (() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.Where (() => orderEquipmentAlias.Direction == Domain.Orders.Direction.Deliver)
				.JoinAlias (() => equipmentAlias.Nomenclature, () => OrderEquipmentNomenclatureAlias)
				.JoinAlias (() => OrderEquipmentNomenclatureAlias.Unit, () => unitsAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where (() => OrderEquipmentNomenclatureAlias.Warehouse == CurrentStock)
				.SelectList (list => list
					.SelectGroup (() => OrderEquipmentNomenclatureAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => OrderEquipmentNomenclatureAlias.Name).WithAlias (() => resultAlias.NomenclatureName)
					.SelectGroup (() => equipmentAlias.Id).WithAlias (() => resultAlias.EquipmentId)
					.SelectSum (() => 1).WithAlias (() => resultAlias.Amount)
					.Select (() => unitsAlias.Name).WithAlias (() => resultAlias.UnitName)
				)
				.TransformUsing (Transformers.AliasToBean <ShipmentItemsNode> ())
				.List<ShipmentItemsNode> ();

			foreach(var node in orderEquipments)
			{
				if (!ShipmentList.Exists (item => item.EquipmentId == node.EquipmentId))
					ShipmentList.Add (node);
			}

			ytreeItems.ItemsDataSource = ShipmentList;
		}

		protected void OnYcomboboxWarehouseItemSelected (object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			UpdateItemsList ();
		}

		public class ShipmentItemsNode
		{
			public int Id{ get; set; }

			public string NomenclatureName { get; set; }

			public string SerialNumber { get{return EquipmentId > 0 ? EquipmentId.ToString () : null;}}

			public int Amount { get; set; }

			public bool IsNew { get; set; }

			public int EquipmentId { get; set; }

			public string UnitName{ get; set;}

			public string SerialNumberText {
				get {return IsNew ? String.Format ("{0}(новый)", SerialNumber) : String.Format ("{0}", SerialNumber); }
			}

			public string AmountText { get { return String.Format ("{0} {1}", 
				Amount,
				UnitName);
				}}
		}

	}
}

