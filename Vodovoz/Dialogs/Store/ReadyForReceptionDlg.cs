using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Widgets;
using NHibernate.Criterion;
using NHibernate.Transform;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Repository;

namespace Vodovoz
{	
	public partial class ReadyForReceptionDlg : TdiTabBase
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot ();

		int shipmentId;
		RouteList routelist;

		GenericObservableList<ReceptionItemNode> ReceptionReturnsList = new GenericObservableList<ReceptionItemNode>();
		GenericObservableList<ReceptionItemNode> ReceptionEquipmentList = new GenericObservableList<ReceptionItemNode>();
		ReceptionItemNode equipmentToRegister;

		public ReadyForReceptionDlg (int id, Warehouse stock)
		{		
			this.Build ();	
			shipmentId = id;

			this.TabName = "Прием машины";

			ycomboboxWarehouse.ItemsList = Repository.Store.WarehouseRepository.WarehouseForShipment (UoW, ShipmentDocumentType.RouteList, id);

			bottleReceptionView.UoW = UoW;

			ytreeEquipment.ItemsDataSource = ReceptionEquipmentList;
			ytreeReturns.ItemsDataSource = ReceptionReturnsList;

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

			routelist = UoW.GetById<RouteList> (id);
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
			ReceptionEquipmentList.Clear ();
			ReceptionReturnsList.Clear ();
			Warehouse CurrentStock = ycomboboxWarehouse.SelectedItem as Warehouse;
			if (CurrentStock == null)
				return;
			
			bottleReceptionView.Visible = CurrentStock.CanReceiveBottles;
			frameEquipment.Visible = CurrentStock.CanReceiveEquipment;
			buttonAddEquipment.Sensitive = buttonConfirmReception.Sensitive = CanUnload ();

			if (CurrentStock.CanReceiveEquipment) {				
				ListEquipment ();
				ListTrackableEquipment ();
				ListNewTrackableEquipment ();
			}
			ListReturnableItems ();
		}
			
		void ListEquipment(){
			ReceptionItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
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

		void ListNewTrackableEquipment(){
			ReceptionItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature nomenclatureAlias = null;
			var equipmentItems = UoW.Session.QueryOver<RouteListItem> ().Where (r => r.RouteList.Id == shipmentId)
				.JoinAlias (rli => rli.Order, () => orderAlias)
				.JoinAlias (() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.Where(()=>orderEquipmentAlias.Direction==Domain.Orders.Direction.PickUp)
				.JoinAlias(()=>orderEquipmentAlias.NewEquipmentNomenclature,()=>nomenclatureAlias)
				.Where(()=>nomenclatureAlias.Serial)
				.SelectList (list => list					
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

			var returnableItems = UoW.Session.QueryOver<RouteListItem> ().Where (r => r.RouteList.Id == shipmentId)
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

			var returnableEquipment = UoW.Session.QueryOver<RouteListItem> ().Where (r => r.RouteList.Id == shipmentId)
				.JoinAlias (rli => rli.Order, () => orderAlias)
				.JoinAlias (() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.JoinAlias (() => orderEquipmentAlias.Equipment, () => equipmentAlias)
				.JoinAlias (() => equipmentAlias.Nomenclature, () => nomenclatureAlias)
				.Where(()=>orderEquipmentAlias.Direction==Vodovoz.Domain.Orders.Direction.Deliver)
				.Where (Restrictions.Or (
				                        Restrictions.On (() => nomenclatureAlias.Warehouse).IsNull,
				                        Restrictions.Eq (Projections.Property (() => nomenclatureAlias.Warehouse), warehouse)
			                        ))
				.Where (() => nomenclatureAlias.Category != NomenclatureCategory.rent
			                        && nomenclatureAlias.Category != NomenclatureCategory.deposit)				
				.SelectList (list => list
					.Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.Id)				
					.Select (() => nomenclatureAlias.Id).WithAlias (() => resultAlias.NomenclatureId)
					.Select (() => nomenclatureAlias.Name).WithAlias (() => resultAlias.Name)
					.Select (() => nomenclatureAlias.Serial).WithAlias (() => resultAlias.Trackable)
					.Select (() => nomenclatureAlias.Category).WithAlias (() => resultAlias.NomenclatureCategory)
			                        )
				.TransformUsing (Transformers.AliasToBean<ReceptionItemNode> ())
				.List<ReceptionItemNode> ();
			
			foreach (var item in returnableItems.Union(returnableEquipment)) {			 
				ReceptionReturnsList.Add (item);
			}				
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

			foreach (Vodovoz.ViewModel.BottleReceptionVMNode node in bottleReceptionView.Items) {
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
			CarUnloadDocumentUoW.Save ();

			routelist.Receive();
			UoW.Save (routelist);
			UoW.Commit ();

			logger.Info("Печать разгрузочного талона...");

			var reportInfo = new QSReport.ReportInfo {
				Title = CarUnloadDocumentUoW.Root.Title,
				Identifier = "Store.CarUnloadDoc",
				Parameters = new Dictionary<string, object> {
					{ "id",  CarUnloadDocumentUoW.Root.Id }
				}
			};

			var report = new QSReport.ReportViewDlg (reportInfo);
			TabParent.AddTab (report, this, false);

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
}

