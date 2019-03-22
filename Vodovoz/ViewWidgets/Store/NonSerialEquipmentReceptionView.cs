using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Gtk;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;

namespace Vodovoz.ViewWidgets.Store
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class NonSerialEquipmentReceptionView : QS.Dialog.Gtk.WidgetOnDialogBase
	{
		GenericObservableList<ReceptionNonSerialEquipmentItemNode> ReceptionNonSerialEquipmentList = new GenericObservableList<ReceptionNonSerialEquipmentItemNode>();

		public IList<ReceptionNonSerialEquipmentItemNode> Items {
			get {
				return ReceptionNonSerialEquipmentList;
			}
		}

		public NonSerialEquipmentReceptionView()
		{
			this.Build();

			ytreeEquipment.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionNonSerialEquipmentItemNode>()
				.AddColumn("Номенклатура").AddTextRenderer(node => node.Name)
				.AddColumn("Забирали").AddNumericRenderer(node => node.NeedReceptionCount)
				.AddColumn("Получено ")
				.AddNumericRenderer(node => node.Amount, false)
					.Adjustment(new Adjustment(0, 0, 10000, 1, 10, 10))
					.Editing()
				.AddColumn("")
				.Finish();
			
			ytreeEquipment.ItemsDataSource = ReceptionNonSerialEquipmentList;
		}

		RouteList routeList;
		public RouteList RouteList {
			get {
				return routeList;
			}
			set {
				if(routeList == value)
					return;
				routeList = value;
				if(routeList != null) {
					FillListEquipmentFromRoute();
				} else {
					ReceptionNonSerialEquipmentList.Clear();
				}

			}
		}

		void FillListEquipmentFromRoute()
		{
			ReceptionNonSerialEquipmentList.Clear();
			ReceptionNonSerialEquipmentItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature NomenclatureAlias = null;
			var equipmentItems = MyOrmDialog.UoW.Session.QueryOver<RouteListItem>()
														.Where(r => r.RouteList.Id == RouteList.Id)
														.JoinAlias(rli => rli.Order, () => orderAlias)
														.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
														.Where(() => orderEquipmentAlias.Direction == Domain.Orders.Direction.PickUp)
														.JoinAlias(() => orderEquipmentAlias.Nomenclature, () => NomenclatureAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
														.SelectList(list => list
														   .SelectGroup(() => NomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
														   .Select(() => NomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
														   .SelectSum(() => orderEquipmentAlias.Count).WithAlias(() => resultAlias.NeedReceptionCount)
														)
														.TransformUsing(Transformers.AliasToBean<ReceptionNonSerialEquipmentItemNode>())
														.List<ReceptionNonSerialEquipmentItemNode>();

			foreach(var equipment in equipmentItems)
				ReceptionNonSerialEquipmentList.Add(equipment);
		}

		protected void OnButtonAddEquipmentClicked(object sender, EventArgs e)
		{
			OrmReference refWin = new OrmReference(NomenclatureRepository.NomenclatureByCategory(NomenclatureCategory.equipment));
			refWin.FilterClass = null;
			refWin.Mode = OrmReferenceMode.Select;
			refWin.ObjectSelected += RefWin_ObjectSelected;
			MyTab.TabParent.AddTab(refWin, MyTab);
		}

		void RefWin_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			Nomenclature nomenclature = (e.Subject as Nomenclature);
			if(nomenclature == null) {
				return;
			}
			var node = new ReceptionNonSerialEquipmentItemNode() {
				NomenclatureCategory = nomenclature.Category,
				NomenclatureId = nomenclature.Id,
				Name = nomenclature.Name
			};
			ReceptionNonSerialEquipmentList.Add(node);
		}
	}

	public class ReceptionNonSerialEquipmentItemNode : PropertyChangedBase
	{
		public NomenclatureCategory NomenclatureCategory { get; set; }
		public int NomenclatureId { get; set; }
		public string Name { get; set; }

		public int NeedReceptionCount { get; set; }

		int amount;
		public virtual int Amount {
			get { return amount; }
			set {
				SetField(ref amount, value, () => Amount);
			}
		}

		int returned;
		public int Returned {
			get {
				return returned;
			}
			set {
				SetField(ref returned, value, () => Returned);
			}
		}
	}
}
