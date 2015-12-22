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
using Vodovoz.Domain.Operations;
using QSProjectsLib;
using Vodovoz.Domain.Documents;

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
			if (CurrentStock.CanReceiveEquipment) {				
				ListEquipment ();
				ListTrackableEquipment ();
			}
			ListReturnableItems ();
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

		/*
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
		*/

		void ListEquipment(){
			ReceptionItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			ServiceClaim serviceClaimAlias = null;
			var equipmentItems = UoW.Session.QueryOver<RouteListItem> ().Where (r => r.RouteList.Id == shipmentId)
				.JoinAlias (rli => rli.Order, () => orderAlias)
				.JoinAlias (() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
					.Where(()=>orderEquipmentAlias.Direction==Domain.Orders.Direction.PickUp)
				.JoinAlias (() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.JoinAlias(()=>equipmentAlias.Nomenclature,()=>nomenclatureAlias)
					.Where(()=>!nomenclatureAlias.Serial)
				.SelectList (list => list
					.Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.Id)
					.SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => nomenclatureAlias.Serial).WithAlias (() => resultAlias.Trackable)
				)
				.TransformUsing (Transformers.AliasToBean<ReceptionItemNode> ())
				.List<ReceptionItemNode> ();
			foreach (var equipment in equipmentItems)
				ReceptionEquipmentList.Add (equipment);		
		}

		void ListTrackableEquipment()
		{
			ReceptionItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			ServiceClaim serviceClaimAlias = null;
			var equipmentItems = UoW.Session.QueryOver<RouteListItem> ().Where (r => r.RouteList.Id == shipmentId)
				.JoinAlias (rli => rli.Order, () => orderAlias)
				.JoinAlias (() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
					.Where(()=>orderEquipmentAlias.Direction==Domain.Orders.Direction.PickUp)
				.JoinAlias (() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.JoinAlias(()=>equipmentAlias.Nomenclature,()=>nomenclatureAlias)
					.Where(()=>nomenclatureAlias.Serial)
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

		void ListReturnableItems(){
			ReceptionItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrderItem orderItemsAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Warehouse warehouse = ycomboboxWarehouse.SelectedItem as Warehouse;

			var returnedItems = UoW.Session.QueryOver<RouteListItem> ().Where (r => r.RouteList.Id == shipmentId)
				.JoinAlias (rli => rli.Order, () => orderAlias)
				.JoinAlias (() => orderAlias.OrderItems, () => orderItemsAlias)
				.JoinAlias (() => orderItemsAlias.Nomenclature, () => nomenclatureAlias)
				.Where (() => !nomenclatureAlias.Serial)
				.Where (Restrictions.Or (
				                    Restrictions.On (() => nomenclatureAlias.Warehouse).IsNull,
				                    Restrictions.Eq (Projections.Property (() => nomenclatureAlias.Warehouse), warehouse)
			                    ))
				.Where (() => nomenclatureAlias.Category != NomenclatureCategory.rent
			                    && nomenclatureAlias.Category != NomenclatureCategory.deposit)
				.SelectList (list => list					
					.SelectGroup (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => false).WithAlias (() => resultAlias.Trackable)
					.Select (() => nomenclatureAlias.Category).WithAlias (() => resultAlias.NomenclatureCategory)
			                    )
				.TransformUsing (Transformers.AliasToBean<ReceptionItemNode> ())
				.List<ReceptionItemNode> ();

			var returnedEquipment = UoW.Session.QueryOver<RouteListItem> ().Where (r => r.RouteList.Id == shipmentId)
				.JoinAlias (rli => rli.Order, () => orderAlias)
				.JoinAlias (() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.JoinAlias (() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.JoinAlias (() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where (Restrictions.Or (
				                        Restrictions.On (() => nomenclatureAlias.Warehouse).IsNull,
				                        Restrictions.Eq (Projections.Property (() => nomenclatureAlias.Warehouse), warehouse)
			                        ))
				.Where (() => nomenclatureAlias.Category != NomenclatureCategory.rent
			                        && nomenclatureAlias.Category != NomenclatureCategory.deposit)
				.Where (() => orderEquipmentAlias.Direction == Vodovoz.Domain.Orders.Direction.Deliver)
				.SelectList (list => list
					.Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.Id)				
					.Select (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => nomenclatureAlias.Serial).WithAlias (() => resultAlias.Trackable)
					.Select (() => nomenclatureAlias.Category).WithAlias (() => resultAlias.NomenclatureCategory)
			                        )
				.TransformUsing (Transformers.AliasToBean<ReceptionItemNode> ())
				.List<ReceptionItemNode> ();
			
			foreach (var item in returnedItems.Union(returnedEquipment)) {			 
				ReceptionReturnsList.Add (item);
			}

			buttonConfirmReception.Sensitive = CanUnload ();
		}

		protected bool CanUnload(){
			var warehouse = ycomboboxWarehouse.SelectedItem as Warehouse;
			return UoW.Session.QueryOver<CarUnloadDocument> ()
				.Where (doc => doc.Warehouse.Id == warehouse.Id)
				.And (doc => doc.RouteList.Id == shipmentId)
				.RowCount()==0;
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

		protected void OnButtonConfirmReceptionClicked (object sender, EventArgs e)
		{			
			var CarUnloadDocumentUoW = UnitOfWorkFactory.CreateWithNewRoot <CarUnloadDocument> ();

			CarUnloadDocumentUoW.Root.Storekeeper = EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if (CarUnloadDocumentUoW.Root.Storekeeper == null) {
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете загружать автомобили, так как некого указывать в качестве кладовщика.");
				return;
			}

			CarUnloadDocumentUoW.Root.RouteList = UoW.GetById<RouteList> (shipmentId);
			CarUnloadDocumentUoW.Root.Warehouse = ycomboboxWarehouse.SelectedItem as Warehouse;

			foreach (ReceptionBottleNode node in ReceptionBottleList) {
				if (node.Amount != 0) {
					var warehouseMovementOperation = UnitOfWorkFactory.CreateWithNewRoot <WarehouseMovementOperation> ();
					warehouseMovementOperation.Root.Amount = node.Amount;
					warehouseMovementOperation.Root.Nomenclature = UoW.GetById <Nomenclature> (node.NomenclatureId);
					warehouseMovementOperation.Root.IncomingWarehouse = ycomboboxWarehouse.SelectedItem as Warehouse;
					warehouseMovementOperation.Root.OperationTime = DateTime.Now;
					warehouseMovementOperation.Save ();
					CarUnloadDocumentUoW.Root.AddItem (new CarUnloadDocumentItem { MovementOperation = warehouseMovementOperation.Root });
				}
			}

			foreach (ReceptionItemNode node in ReceptionEquipmentList.Union(ReceptionReturnsList)) {
				if (node.Amount != 0) {
					var warehouseMovementOperation = UnitOfWorkFactory.CreateWithNewRoot <WarehouseMovementOperation> ();
					warehouseMovementOperation.Root.Amount = node.Amount;
					warehouseMovementOperation.Root.Equipment = UoW.GetById<Equipment> (node.Id);				
					warehouseMovementOperation.Root.Nomenclature = UoW.GetById<Nomenclature> (node.NomenclatureId);				
					warehouseMovementOperation.Root.IncomingWarehouse = ycomboboxWarehouse.SelectedItem as Warehouse;
					warehouseMovementOperation.Root.OperationTime = DateTime.Now;
					warehouseMovementOperation.Save ();
					CarUnloadDocumentUoW.Root.AddItem (new CarUnloadDocumentItem { MovementOperation = warehouseMovementOperation.Root });
				}
			}
			RouteList routelist = CarUnloadDocumentUoW.GetById<RouteList> (shipmentId);
			ChangeRouteListStatus (routelist);
			CarUnloadDocumentUoW.Save (routelist);
			CarUnloadDocumentUoW.Save ();
			OnCloseTab (false);
		}

		protected void ChangeRouteListStatus(RouteList routelist){
			routelist.Status = RouteListStatus.ReadyToReport;
			foreach (var item in routelist.Addresses) {
				item.Order.OrderStatus = OrderStatus.UnloadingOnStock;
			}

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

