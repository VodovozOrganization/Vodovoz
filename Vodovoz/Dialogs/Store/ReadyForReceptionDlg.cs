using System;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain.Store;
using Vodovoz.Domain;
using Gamma.Widgets;
using Vodovoz.Domain.Logistic;
using System.Data.Bindings;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using Vodovoz.Domain.Service;
using System.Data.Bindings.Collections.Generic;
using System.Linq;

namespace Vodovoz
{	
	public partial class ReadyForReceptionDlg : TdiTabBase
	{
		private IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot ();

		int shipmentId;

		GenericObservableList<ReceptionItemNode> ReceptionReturnsList = new GenericObservableList<ReceptionItemNode>();
		GenericObservableList<ReceptionItemNode> ReceptionEquipmentList = new GenericObservableList<ReceptionItemNode>();
		GenericObservableList<ReceptionBottleNode> ReceptionBottleList = new GenericObservableList<ReceptionBottleNode>();
		ReceptionItemNode equipmentToRegister;

		public ReadyForReceptionDlg ()
		{
			this.Build ();
		}

		public ReadyForReceptionDlg (int id, Warehouse stock):this()
		{			
			shipmentId = id;

			this.TabName = "Прием машины";

			ycomboboxWarehouse.ItemsList = Repository.Store.WarehouseRepository.WarehouseForShipment (UoW, ShipmentDocumentType.RouteList, id);

			ytreeBottles.ItemsDataSource = ReceptionBottleList;
			ytreeEquipment.ItemsDataSource = ReceptionEquipmentList;
			ytreeReturns.ItemsDataSource = ReceptionReturnsList;

			ytreeBottles.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionBottleNode> ()
				.AddColumn ("Номенклатура").AddTextRenderer (node => node.Name)
				.AddColumn ("Кол-во").AddNumericRenderer (node => node.Amount)
					.Adjustment (new Gtk.Adjustment (0, 0, 9999, 1, 100, 0))
					.Editing (true)
				.AddColumn("")
				.Finish ();

			ytreeEquipment.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionItemNode> ()
				.AddColumn ("Номенклатура").AddTextRenderer (node => node.Name)
				.AddColumn ("Серийный номер").AddTextRenderer (node => node.Serial)
				.AddColumn ("Кол-во")
					.AddToggleRenderer (node => node.Returned, false)						
						.AddSetter ((cell, node) => cell.Visible = node.Trackable)
					.AddNumericRenderer (node => node.Amount, false)
					.Adjustment (new Gtk.Adjustment (0, 0, 9999, 1, 100, 0))
						.AddSetter ((cell, node) => cell.Editable = !node.Trackable)
				.AddColumn("")
				.Finish ();
			
			ytreeReturns.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionItemNode> ()
				.AddColumn ("Номенклатура").AddTextRenderer (node => node.Name)
				.AddColumn ("Серийный номер").AddTextRenderer (node => node.Serial)
				.AddColumn ("Кол-во")
					.AddToggleRenderer (node => node.Returned, false)						
						.AddSetter ((cell, node) => cell.Visible = node.Trackable)
					.AddNumericRenderer (node => node.Amount, false)
						.Adjustment (new Gtk.Adjustment (0, 0, 9999, 1, 100, 0))
						.AddSetter ((cell, node) => cell.Editable = !node.Trackable)
				.AddColumn ("")
				.Finish ();

			if (stock != null)
				ycomboboxWarehouse.SelectedItem = stock;

			var routelist = UoW.GetById<RouteList> (id);
			textviewShipmentInfo.Buffer.Text =
					String.Format ("Маршрутный лист №{0} от {1:d}\nВодитель: {2}\nМашина: {3}({4})\nЭкспедитор: {5}",
				id,
				routelist.Date,
				routelist.Driver.FullName,
				routelist.Car.Model,
				routelist.Car.RegistrationNumber,
				routelist.Forwarder != null ? routelist.Forwarder.FullName : "(Отсутствует)" 
			);
			TabName = String.Format ("Выгрузка маршрутного листа №{0}", id);
		}

		protected void OnYcomboboxWarehouseItemSelected (object sender, ItemSelectedEventArgs e)
		{
			UpdateItemsList ();
		}

		void UpdateItemsList(){
			ReceptionBottleList.Clear ();
			ReceptionEquipmentList.Clear ();
			ReceptionReturnsList.Clear ();
			Warehouse CurrentStock = ycomboboxWarehouse.SelectedItem as Warehouse;
			if (CurrentStock == null)
				return;
			ytreeBottles.Sensitive = CurrentStock.CanReceiveBottles;
			ytreeEquipment.Sensitive = CurrentStock.CanReceiveEquipment;
			if (CurrentStock.CanReceiveBottles)
				ListBottles ();
			if (CurrentStock.CanReceiveEquipment) 
				ListEquipmentForService ();
			var returnableItems = GetReturnableItems (CurrentStock);
			returnableItems.ToList ().ForEach (item => 
				ReceptionReturnsList.Add (item)
			);
		}

		void ListBottles ()
		{
			ReceptionBottleNode resultAlias = null;
			Nomenclature nomenclatureAlias = null;

			var orderBottles = UoW.Session.QueryOver<Nomenclature> (() => nomenclatureAlias).Where (n => n.Category == NomenclatureCategory.bottle)
				.SelectList (list => list
					.Select (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
				).TransformUsing (Transformers.AliasToBean<ReceptionBottleNode> ())
				.List<ReceptionBottleNode> ();

			foreach (var bottle in orderBottles)
				ReceptionBottleList.Add (bottle);			
		}

		IList<ReceptionItemNode> GetReturnableItems(Warehouse warehouse){
			ReceptionItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemsAlias = null;

			var returnedItems = UoW.Session.QueryOver<RouteListItem> ().Where (r => r.RouteList.Id == shipmentId)
				.JoinAlias (rli => rli.Order, () => orderAlias)
				.JoinAlias (() => orderAlias.OrderItems, () => orderItemsAlias)
				.JoinAlias (() => orderItemsAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias (() => orderItemsAlias.Equipment, () => equipmentAlias,NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(Restrictions.Or(
					Restrictions.On(()=>nomenclatureAlias.Warehouse).IsNull,
					Restrictions.Eq(Projections.Property(()=>nomenclatureAlias.Warehouse),warehouse)
				))
				.Where(()=>nomenclatureAlias.Category!=NomenclatureCategory.rent
					&& nomenclatureAlias.Category!=NomenclatureCategory.deposit)
				.SelectList (list => list
					.Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.Id)				
					.SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => nomenclatureAlias.Serial).WithAlias (() => resultAlias.Trackable)
					.Select(()=>nomenclatureAlias.Category).WithAlias(()=>resultAlias.NomenclatureCategory)
				)
				.TransformUsing (Transformers.AliasToBean<ReceptionItemNode> ())
				.List<ReceptionItemNode> ();
			return returnedItems;
		}

		void ListEquipmentForService(){
			ReceptionItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			ServiceClaim serviceClaimAlias = null;
		
			var equipmentItems = UoW.Session.QueryOver<RouteListItem> ().Where (r => r.RouteList.Id == shipmentId)
				.JoinAlias (rli => rli.Order, () => orderAlias)
				.JoinAlias (() => orderAlias.InitialOrderService, () => serviceClaimAlias)
				.JoinAlias (() => serviceClaimAlias.Equipment, () => equipmentAlias,NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias (() => serviceClaimAlias.Nomenclature, () => nomenclatureAlias)
				.SelectList (list => list
					.Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.Id)
					.Select (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => nomenclatureAlias.Serial).WithAlias (() => resultAlias.Trackable)
			                     )
				.TransformUsing (Transformers.AliasToBean<ReceptionItemNode> ())
				.List<ReceptionItemNode> ();
			foreach (var equipment in equipmentItems)
				ReceptionEquipmentList.Add (equipment);			
		}

		protected void OnButtonAddEquipmentClicked (object sender, EventArgs e)
		{
			var dlg = new OrmReference (typeof(Equipment), UoW);
			dlg.Mode = OrmReferenceMode.Select;
			dlg.ObjectSelected += OnEquipmentAdded;
			TabParent.AddSlaveTab (this, dlg);
		}

		protected void OnEquipmentAdded(object sender, OrmReferenceObjectSectedEventArgs args){
			var equipment = args.Subject as Equipment;
			var equipmentNode = new ReceptionItemNode {
				Id = equipment.Id,
				NomenclatureId = equipment.Nomenclature.Id,
				Name = equipment.NomenclatureName,
				Trackable=true,
				Returned = true
			};
			ReceptionEquipmentList.Add (equipmentNode);
		}

		protected void OnYtreeEquipmentRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			var itemNode = ytreeEquipment.GetSelectedObject () as ReceptionItemNode;
			if (itemNode.Trackable && itemNode.Id==0) {
				var dlg = EquipmentGenerator.CreateOne (itemNode.NomenclatureId);
				dlg.EquipmentCreated += OnEquipmentRegistered;
				if (!TabParent.CheckClosingSlaveTabs (this)) {					
					equipmentToRegister = itemNode;
					TabParent.AddSlaveTab (this, dlg);
				}
			}
		}

		protected void OnEquipmentRegistered(object o, EquipmentCreatedEventArgs args){
			equipmentToRegister.Id = args.Equipment [0].Id;
			equipmentToRegister.Returned = true;
		}
	}

	public class ReceptionItemNode{
		public int Id{get;set;}
		public NomenclatureCategory NomenclatureCategory{ get; set; }
		public int NomenclatureId{ get; set; }
		public string Name{get;set;}
		public int Amount{ get; set;}
		public bool Trackable{ get; set; }
		public string Serial{ get { 			
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

	public class ReceptionBottleNode{
		public int NomenclatureId{get;set;}
		public string Name{get;set;}
		public int Amount{get;set;}
	}
}

