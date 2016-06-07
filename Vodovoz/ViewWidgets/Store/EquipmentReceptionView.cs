using System;
using QSOrmProject;
using Vodovoz.Domain.Logistic;
using System.Collections.Generic;
using Vodovoz.Domain.Service;
using System.Linq;
using Gtk;
using Vodovoz.Domain.Goods;
using Vodovoz.Repository;
using QSProjectsLib;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EquipmentReceptionView : WidgetOnDialogBase
	{
		IList<ServiceClaim> serviceClaims;

		MenuItem menuitemSelectFromClient;
		MenuItem menuitemRegisterSerial;

		ReceptionItemNode equipmentToSetSerial;

		public EquipmentReceptionView()
		{
			this.Build();

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
		}

		RouteList routeList;
		public RouteList RouteList
		{
			get
			{
				return routeList;
			}
			set
			{
				if (routeList == value)
					return;
				routeList = value;
				if (routeList != null)
					serviceClaims = RouteList.Addresses
					.SelectMany(address => address.Order.InitialOrderService)
					.ToList();
				else
					serviceClaims = new List<ServiceClaim>();
			}
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

		void MenuitemSelectFromUnused_Activated (object sender, EventArgs e)
		{
			equipmentToSetSerial = ytreeEquipment.GetSelectedObject<ReceptionItemNode>();
			var nomenclature = MyOrmDialog.UoW.GetById<Nomenclature>(equipmentToSetSerial.NomenclatureId);
			var selectUnusedEquipment = new OrmReference(EquipmentRepository.GetUnusedEquipment(nomenclature));
			selectUnusedEquipment.ObjectSelected += SelectUnusedEquipment_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, selectUnusedEquipment);
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
			MyTab.TabParent.AddSlaveTab(MyTab, selectFromClientDlg);
		}

		void SelectUnusedEquipment_ObjectSelected (object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var equipment = MyOrmDialog.UoW.GetById<Equipment>((e.Subject as Equipment).Id);
			equipmentToSetSerial.NewEquipment = equipment;
			equipmentToSetSerial.Id = equipment.Id;
			equipmentToSetSerial.Returned = true;
			//OnEquipmentListChanged();
		}

		void SelectFromClientDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var equipment = MyOrmDialog.UoW.GetById<Equipment>(e.ObjectId);
			equipmentToSetSerial.NewEquipment = equipment;
			equipmentToSetSerial.Id = equipment.Id;
			equipmentToSetSerial.Returned = true;
			//OnEquipmentListChanged();
		}

		void MenuitemRegisterSerial_Activated (object sender, EventArgs e)
		{
			RegisterSerial();
		}

		private void RegisterSerial()
		{
			var itemNode = ytreeEquipment.GetSelectedObject () as ReceptionItemNode;
			if (itemNode.IsNew && itemNode.Id==0) {
				var dlg = EquipmentGenerator.CreateOne (itemNode.NomenclatureId);
				dlg.EquipmentCreated += OnEquipmentRegistered;
				if (!MyTab.TabParent.CheckClosingSlaveTabs (MyTab)) {					
					equipmentToSetSerial = itemNode;
					MyTab.TabParent.AddSlaveTab (MyTab, dlg);
				}
			}
		}

		protected void OnEquipmentRegistered(object o, EquipmentCreatedEventArgs args){
			var equipment = MyOrmDialog.UoW.GetById<Equipment>(args.Equipment[0].Id);
			equipmentToSetSerial.NewEquipment = equipment;
			equipmentToSetSerial.Id = equipment.Id;
			equipmentToSetSerial.Returned = true;
			//OnEquipmentListChanged();
		}

	}
}

