using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Widgets;
using Gtk;
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
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Store;
using Vodovoz.Repository;

namespace Vodovoz
{	
	public partial class ReadyForReceptionDlg : TdiTabBase
	{
		protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot ();

		int shipmentId;
		Warehouse warehouse;
		RouteList routelist;
		IList<ServiceClaim> serviceClaims;

		GenericObservableList<ReceptionItemNode> ReceptionReturnsList = new GenericObservableList<ReceptionItemNode>();
		GenericObservableList<ReceptionItemNode> ReceptionEquipmentList = new GenericObservableList<ReceptionItemNode>();
		IList<Equipment> alreadyUnloadedEquipment;

		ReceptionItemNode equipmentToSetSerial;

		MenuItem menuitemSelectFromClient;
		MenuItem menuitemRegisterSerial;

		public ReadyForReceptionDlg (int id, Warehouse stock)
		{		
			this.Build ();	
			shipmentId = id;

			this.TabName = "Прием машины";

			ycomboboxWarehouse.ItemsList = Repository.Store.WarehouseRepository.WarehouseForReception (UoW, ShipmentDocumentType.RouteList, id);
			warehouse = UoW.GetById<Warehouse>(shipmentId);
			routelist = UoW.GetById<RouteList> (id);
			serviceClaims = routelist.Addresses
				.SelectMany(address => address.Order.InitialOrderService)
				.ToList();

			bottleReceptionView.UoW = UoW;

			ytreeEquipment.ItemsDataSource = ReceptionEquipmentList;
			ytreeReturns.ItemsDataSource = ReceptionReturnsList;

			ReceptionEquipmentList.ElementChanged += (list, aIdx) => OnEquipmentListChanged();

			ytreeEquipment.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionItemNode> ()
				.AddColumn ("Номенклатура").AddTextRenderer (node => node.Name)
				.AddColumn ("Серийный номер").AddTextRenderer (node => node.Serial)
				.AddColumn ("Кол-во")
					.AddToggleRenderer (node => node.Returned, false)						
						.AddSetter ((cell, node) => cell.Visible = node.Trackable)
					.AddNumericRenderer (node => node.Amount, false)
					.Adjustment (new Gtk.Adjustment (0, 0, 9999, 1, 100, 0))
						.AddSetter ((cell, node) => cell.Editable = !node.Trackable)
				.AddColumn("Заявка на сервис")
					.AddComboRenderer(node=>node.ServiceClaim)
						.Editing()
						.SetDisplayFunc(service=>{
							var serviceClaim = service as ServiceClaim;
							var orderId = serviceClaim.InitialOrder.Id;
							return String.Format("Заявка №{0}, заказ №{1}",serviceClaim.Id,orderId);
						})
						.FillItems<ServiceClaim>(serviceClaims.Where(sc=>sc.Equipment==null).ToList())
						.AddSetter((cell,node)=>cell.Sensitive = node.IsNew)
						.AddSetter((cell,node)=>cell.Editable = node.IsNew)
				.AddColumn("")
				.Finish ();

			ytreeEquipment.Selection.Changed += YtreeEquipment_Selection_Changed;
			
			ytreeReturns.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionItemNode> ()
				.AddColumn ("Номенклатура").AddTextRenderer (node => node.Name)
				.AddColumn ("Серийный номер").AddTextRenderer (node => node.Serial)
				.AddColumn ("Кол-во")
					.AddToggleRenderer (node => node.Returned, false)						
						.AddSetter ((cell, node) => cell.Visible = node.Trackable)
					.AddNumericRenderer (node => node.Amount, false)
						.Adjustment (new Gtk.Adjustment (0, 0, 9999, 1, 100, 0))
						.AddSetter ((cell, node) => cell.Editable = node.Id==0)
				.AddColumn ("")
				.Finish ();

			if (stock != null)
				ycomboboxWarehouse.SelectedItem = stock;

			//Создаем меню в кнопке выбора СН
			var menu = new Menu();
			menuitemRegisterSerial = new MenuItem("Зарегистрировать новый СН");
			menuitemRegisterSerial.Activated += MenuitemRegisterSerial_Activated;
			menu.Add(menuitemRegisterSerial);
			menuitemSelectFromClient = new MenuItem("Выбрать по клиенту");
			menuitemSelectFromClient.Activated += MenuitemSelectFromClient_Activated;
			menu.Add(menuitemSelectFromClient);
			var menuitemSelectFromUnused = new MenuItem("Незадействованные СН");
			menuitemSelectFromUnused.Activated += MenuitemSelectFromUnused_Activated;
			menu.Add(menuitemSelectFromUnused);
			menu.ShowAll();
			buttonSelectSerial.Menu = menu;

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

		void MenuitemSelectFromUnused_Activated (object sender, EventArgs e)
		{
			equipmentToSetSerial = ytreeEquipment.GetSelectedObject<ReceptionItemNode>();
			var nomenclature = UoW.GetById<Nomenclature>(equipmentToSetSerial.NomenclatureId);
			var selectUnusedEquipment = new OrmReference(EquipmentRepository.GetUnusedEquipment(nomenclature));
			selectUnusedEquipment.ObjectSelected += SelectUnusedEquipment_ObjectSelected;
			TabParent.AddSlaveTab(this, selectUnusedEquipment);
		}

		void MenuitemSelectFromClient_Activated (object sender, EventArgs e)
		{
			equipmentToSetSerial = ytreeEquipment.GetSelectedObject<ReceptionItemNode>();
			var filter = new ClientBalanceFilter(UnitOfWorkFactory.CreateWithoutRoot());
			filter.RestrictCounterparty = equipmentToSetSerial.ServiceClaim.Counterparty;
			filter.RestrictNomenclature = filter.UoW.GetById<Nomenclature>(equipmentToSetSerial.NomenclatureId);
			var selectFromClientDlg = new ReferenceRepresentation(new Vodovoz.ViewModel.ClientEquipmentBalanceVM(filter));
			selectFromClientDlg.TabName = String.Format("Оборудование у {0}", 
				StringWorks.EllipsizeEnd(equipmentToSetSerial.ServiceClaim.Counterparty.Name, 50));
			selectFromClientDlg.ObjectSelected += SelectFromClientDlg_ObjectSelected;
			TabParent.AddSlaveTab(this, selectFromClientDlg);
		}

		void SelectUnusedEquipment_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var equipment = UoW.GetById<Equipment>((e.Subject as Equipment).Id);
			equipmentToSetSerial.NewEquipment = equipment;
			equipmentToSetSerial.Id = equipment.Id;
			equipmentToSetSerial.Returned = true;
			OnEquipmentListChanged();
		}

		void SelectFromClientDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var equipment = UoW.GetById<Equipment>(e.ObjectId);
			equipmentToSetSerial.NewEquipment = equipment;
			equipmentToSetSerial.Id = equipment.Id;
			equipmentToSetSerial.Returned = true;
			OnEquipmentListChanged();
		}

		void MenuitemRegisterSerial_Activated (object sender, EventArgs e)
		{
			RegisterSerial();
		}

		void YtreeEquipment_Selection_Changed (object sender, EventArgs e)
		{
			buttonSelectSerial.Sensitive 
				= ytreeEquipment.Selection.CountSelectedRows() > 0;

			var item = ytreeEquipment.GetSelectedObject<ReceptionItemNode>();

			if (item != null && menuitemSelectFromClient != null)
				menuitemSelectFromClient.Sensitive = item.ServiceClaim != null;
			if (item != null && menuitemRegisterSerial != null)
				menuitemRegisterSerial.Sensitive = item.IsNew;
		}

		void OnEquipmentListChanged()
		{
			var noSerialCount = ReceptionEquipmentList
				.Where(item => item.Returned)
				.Where(item => item.Id == 0)
				.Count();

			var noServiceClaimCount = ReceptionEquipmentList
				.Where(item=>item.Returned)
				.Where(item => item.IsNew && item.ServiceClaim == null)
				.Count();

			var hasDublicateServiceClaims = ReceptionEquipmentList
				.Where(item => item.ServiceClaim != null)
				.GroupBy(item => item.ServiceClaim)
				.Any(g => g.Count() > 1);

			if (noSerialCount > 0)
			{
				labelError.Markup = "<span foreground=\"#ff0000\">Необходимо зарегистрировать поступившее на склад оборудование!</span>";
			}
			else if (noServiceClaimCount > 0)
			{
				labelError.Markup = "<span foreground=\"#ff0000\">Не для всего оборудования указана заявка по которой оно поступило на склад!</span>";
			}
			else if (hasDublicateServiceClaims)
			{
				labelError.Markup = "<span foreground=\"#ff0000\">Заявка на сервис должна соответствовать только одной единице оборудования!</span>";
			}
			else
				labelError.Markup = "";
			var isValid = noSerialCount==0 && noServiceClaimCount==0;
			buttonConfirmReception.Sensitive = isValid;
		}

		protected void OnYcomboboxWarehouseItemSelected (object sender, ItemSelectedEventArgs e)
		{
			UpdateItemsList ();
			OnEquipmentListChanged();
		}

		void UpdateItemsList(){
			ReceptionEquipmentList.Clear ();
			ReceptionReturnsList.Clear ();
			Warehouse CurrentStock = ycomboboxWarehouse.SelectedItem as Warehouse;
			buttonAddEquipment.Sensitive = buttonConfirmReception.Sensitive = CurrentStock != null;
			if (CurrentStock == null)
				return;
			
			alreadyUnloadedEquipment = Repository.EquipmentRepository.GetEquipmentUnloadedTo(UoW, warehouse, routelist);
			bottleReceptionView.Visible = CurrentStock.CanReceiveBottles;
			frameEquipment.Visible = CurrentStock.CanReceiveEquipment;

			if (CurrentStock.CanReceiveEquipment) {
				ListEquipment ();
				ListTrackableEquipment ();
				ListNewTrackableEquipment ();
				foreach (var eqObj in ReceptionEquipmentList)
				{
					var eq = eqObj as ReceptionItemNode;
					if (!eq.IsNew)
					{
						eq.ServiceClaim = serviceClaims
							.Where(service => service.Equipment != null)
							.FirstOrDefault(service => service.Equipment.Id == eq.Id);
					}
				}
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
				if(!alreadyUnloadedEquipment.Any(eq=>eq.Id==equipment.Id))
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
					.Select (() => true).WithAlias (() => resultAlias.IsNew)
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
			
			foreach (var item in returnableItems) {
				ReceptionReturnsList.Add (item);
			}
			foreach (var equipment in returnableEquipment)
			{
				if (!alreadyUnloadedEquipment.Any(eq => eq.Id == equipment.Id))
					ReceptionReturnsList.Add(equipment);
			}

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
			RegisterSerial();
		}

		protected void OnEquipmentRegistered(object o, EquipmentCreatedEventArgs args){
			var equipment = UoW.GetById<Equipment>(args.Equipment[0].Id);
			equipmentToSetSerial.NewEquipment = equipment;
			equipmentToSetSerial.Id = equipment.Id;
			equipmentToSetSerial.Returned = true;
			OnEquipmentListChanged();
		}

		protected void OnButtonConfirmReceptionClicked (object sender, EventArgs e)
		{			
			var CarUnloadDocumentUoW = UnitOfWorkFactory.CreateWithNewRoot <CarUnloadDocument> ();

			CarUnloadDocumentUoW.Root.Author = EmployeeRepository.GetEmployeeForCurrentUser (UoW);
			if (CarUnloadDocumentUoW.Root.Author == null) {
				MessageDialogWorks.RunErrorDialog ("Ваш пользователь не привязан к действующему сотруднику, вы не можете разгружать автомобили, так как некого указывать в качестве кладовщика.");
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
			if(CarUnloadDocumentUoW.Root.Items.Count>0)
				CarUnloadDocumentUoW.Save ();

			foreach (var node in ReceptionEquipmentList.Where(item=>item.Amount>0))
			{
				node.ServiceClaim.UoW = UoW;
				if (node.IsNew)
				{
					node.NewEquipment.AssignedToClient = node.ServiceClaim.Counterparty;
					UoW.Save(node.NewEquipment);
					node.ServiceClaim.FillNewEquipment(node.NewEquipment);
				}
				//FIXME предположительно нужно возвращать статус заявки если поступление удаляется.
				node.ServiceClaim.AddHistoryRecord(ServiceClaimStatus.DeliveredToWarehouse,
					String.Format("Поступил на склад '{0}', по талону разгрузки №{1} для МЛ №{2}", 
						CarUnloadDocumentUoW.Root.Warehouse.Name,
						CarUnloadDocumentUoW.Root.Id,
						CarUnloadDocumentUoW.Root.RouteList.Id
					)
				);
				UoW.Save(node.ServiceClaim);
			}

			if(checkbuttonFinalUnloading.Active)
				routelist.ConfirmReception();
			UoW.Save (routelist);
			UoW.Commit ();

			if (CarUnloadDocumentUoW.Root.Items.Count > 0)
			{
				logger.Info("Печать разгрузочного талона...");

				var reportInfo = new QSReport.ReportInfo
				{
					Title = CarUnloadDocumentUoW.Root.Title,
					Identifier = "Store.CarUnloadDoc",
					Parameters = new Dictionary<string, object>
					{
						{ "id",  CarUnloadDocumentUoW.Root.Id }
					}
				};

				var report = new QSReport.ReportViewDlg(reportInfo);
				TabParent.AddTab(report, this, false);
			}
			OnCloseTab (false);
		}

		private void RegisterSerial()
		{
			var itemNode = ytreeEquipment.GetSelectedObject () as ReceptionItemNode;
			if (itemNode.IsNew && itemNode.Id==0) {
				var dlg = EquipmentGenerator.CreateOne (itemNode.NomenclatureId);
				dlg.EquipmentCreated += OnEquipmentRegistered;
				if (!TabParent.CheckClosingSlaveTabs (this)) {					
					equipmentToSetSerial = itemNode;
					TabParent.AddSlaveTab (this, dlg);
				}
			}
		}
	}

	public class ReceptionItemNode:PropertyChangedBase{
		public int Id{get;set;}
		public NomenclatureCategory NomenclatureCategory{ get; set; }
		public int NomenclatureId{ get; set; }
		public string Name{get;set;}

		int amount;

		public virtual int Amount {
			get{ return amount;}
			set{
				SetField(ref amount, value, ()=>Amount);
			}
		}

		public bool Trackable{ get; set; }
		public string Serial{ get { 			
				if (Trackable) {
					return Id > 0 ? Id.ToString () : "(не определен)";
				} else
					return String.Empty;
			}
		}

		ServiceClaim serviceClaim;

		public virtual ServiceClaim ServiceClaim {
			get{ return serviceClaim;}
			set{
				SetField(ref serviceClaim, value, ()=>ServiceClaim);
			}
		}

		public bool IsNew{ get; set; }
		public Equipment NewEquipment{get;set;}
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

