using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using NHibernate.Transform;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.Repository;
using NHibernate.Criterion;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EquipmentReceptionView : WidgetOnDialogBase
	{
		IList<ServiceClaim> serviceClaims;

		GenericObservableList<ReceptionEquipmentItemNode> ReceptionEquipmentList = new GenericObservableList<ReceptionEquipmentItemNode>();

		MenuItem menuitemSelectFromClient;
		MenuItem menuitemRegisterSerial;

		ReceptionEquipmentItemNode equipmentToSetSerial;

		string colTitleServiceClaim = "Заявка на сервис";

		public EquipmentReceptionView()
		{
			this.Build();

			ytreeEquipment.ColumnsConfig = Gamma.GtkWidgets.ColumnsConfigFactory.Create<ReceptionEquipmentItemNode> ()
				.AddColumn ("Номенклатура").AddTextRenderer (node => node.Name)
				.AddColumn ("Серийный номер").AddTextRenderer (node => node.Serial)
				.AddColumn ("Кол-во")
				.AddToggleRenderer (node => node.Returned, false)						
				.AddNumericRenderer (node => node.Amount, false)
				.AddColumn(colTitleServiceClaim)
				.AddComboRenderer(node=>node.ServiceClaim)
				.Editing()
				.SetDisplayFunc(service=>{
					var serviceClaim = service as ServiceClaim;
					var orderId = serviceClaim.InitialOrder.Id;
					return String.Format("Заявка №{0}, заказ №{1}",serviceClaim.Id,orderId);
				})
				.AddSetter((cell,node)=>cell.Sensitive = node.IsNew)
				.AddSetter((cell,node)=>cell.Editable = node.IsNew)
				.AddColumn("")
				.Finish ();

			ytreeEquipment.Selection.Changed += YtreeEquipment_Selection_Changed;
			ytreeEquipment.ItemsDataSource = ReceptionEquipmentList;

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
				{
					serviceClaims = RouteList.Addresses
					.SelectMany(address => address.Order.InitialOrderService)
					.ToList();
					var column = (ColumnMapping<ReceptionEquipmentItemNode>)ytreeEquipment.ColumnsConfig.ConfiguredColumns.First(x => x.Title == colTitleServiceClaim);
					var cell = (ComboRendererMapping<ReceptionEquipmentItemNode>) column.ConfiguredRenderersGeneric.First();
					cell.FillItems<ServiceClaim>(serviceClaims.Where(sc=>sc.Equipment==null).ToList());
					FillListEquipmentFromRoute();
				}	
				else
				{
					serviceClaims = new List<ServiceClaim>();
					ReceptionEquipmentList.Clear();
				}
					
			}
		}

		void FillListEquipmentFromRoute(){
			ReceptionEquipmentList.Clear();
			ReceptionEquipmentItemNode resultAlias = null;
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			OrderEquipment orderEquipmentAlias = null;
			Nomenclature equipNomenclatureAlias = null, newEqupNomenclatureAlias = null;
			var equipmentItems = MyOrmDialog.UoW.Session.QueryOver<RouteListItem> ().Where (r => r.RouteList.Id == RouteList.Id)
				.JoinAlias (rli => rli.Order, () => orderAlias)
				.JoinAlias (() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.Where(()=>orderEquipmentAlias.Direction==Domain.Orders.Direction.PickUp)
				.JoinAlias (() => orderEquipmentAlias.Equipment, () => equipmentAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(()=>equipmentAlias.Nomenclature,()=> equipNomenclatureAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(()=> orderEquipmentAlias.NewEquipmentNomenclature, ()=> newEqupNomenclatureAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.SelectList (list => list
					.Select (() => equipmentAlias.Id).WithAlias (() => resultAlias.EquipmentId)
					.Select (Projections.Conditional(
						Restrictions.Where(() => equipNomenclatureAlias.Id == null),
						Projections.Property(() => newEqupNomenclatureAlias.Id),
						Projections.Property(() => equipNomenclatureAlias.Id))).WithAlias (() => resultAlias.NomenclatureId)
					.Select (Projections.Conditional(
						Restrictions.Where(() => equipNomenclatureAlias.Name == null),
						Projections.Property(() => newEqupNomenclatureAlias.Name),
						Projections.Property(() => equipNomenclatureAlias.Name))).WithAlias (() => resultAlias.Name)
					.Select (Projections.Conditional(
						Restrictions.Where(() => equipNomenclatureAlias.Name == null),
						Projections.Constant(true),
						Projections.Constant(false)
					)).WithAlias (() => resultAlias.IsNew)
				)
				.TransformUsing (Transformers.AliasToBean<ReceptionEquipmentItemNode> ())
				.List<ReceptionEquipmentItemNode> ();

			foreach (var equipment in equipmentItems)
				ReceptionEquipmentList.Add (equipment);		
		}

		void YtreeEquipment_Selection_Changed (object sender, EventArgs e)
		{
			buttonSelectSerial.Sensitive 
			= ytreeEquipment.Selection.CountSelectedRows() > 0;

			var item = ytreeEquipment.GetSelectedObject<ReceptionEquipmentItemNode>();

			if (item != null && menuitemSelectFromClient != null)
				menuitemSelectFromClient.Sensitive = item.ServiceClaim != null;
			if (item != null && menuitemRegisterSerial != null)
				menuitemRegisterSerial.Sensitive = item.IsNew;
		}

		void MenuitemSelectFromUnused_Activated (object sender, EventArgs e)
		{
			equipmentToSetSerial = ytreeEquipment.GetSelectedObject<ReceptionEquipmentItemNode>();
			var nomenclature = MyOrmDialog.UoW.GetById<Nomenclature>(equipmentToSetSerial.NomenclatureId);
			var selectUnusedEquipment = new OrmReference(EquipmentRepository.GetUnusedEquipment(nomenclature));
			selectUnusedEquipment.Mode = OrmReferenceMode.Select;
			selectUnusedEquipment.ObjectSelected += SelectUnusedEquipment_ObjectSelected;
			MyTab.TabParent.AddSlaveTab(MyTab, selectUnusedEquipment);
		}

		void MenuitemSelectFromClient_Activated (object sender, EventArgs e)
		{
			equipmentToSetSerial = ytreeEquipment.GetSelectedObject<ReceptionEquipmentItemNode>();
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
			equipmentToSetSerial.EquipmentId = equipment.Id;
			equipmentToSetSerial.Returned = true;
			//OnEquipmentListChanged();
		}

		void SelectFromClientDlg_ObjectSelected (object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			var equipment = MyOrmDialog.UoW.GetById<Equipment>(e.ObjectId);
			equipmentToSetSerial.NewEquipment = equipment;
			equipmentToSetSerial.EquipmentId = equipment.Id;
			equipmentToSetSerial.Returned = true;
			//OnEquipmentListChanged();
		}

		void MenuitemRegisterSerial_Activated (object sender, EventArgs e)
		{
			RegisterSerial();
		}

		private void RegisterSerial()
		{
			var itemNode = ytreeEquipment.GetSelectedObject () as ReceptionEquipmentItemNode;
			if (itemNode.IsNew && itemNode.EquipmentId==0) {
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
			equipmentToSetSerial.EquipmentId = equipment.Id;
			equipmentToSetSerial.Returned = true;
			//OnEquipmentListChanged();
		}

	}

	public class ReceptionEquipmentItemNode : PropertyChangedBase{

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

		int equipmentId;
		[PropertyChangedAlso ("Serial")]
		public int EquipmentId
		{
			get
			{
				return equipmentId;
			}
			set
			{
				SetField (ref equipmentId, value, () => EquipmentId);
			}
		}

		public string Serial{ get { 			
				return EquipmentId > 0 ? EquipmentId.ToString () : "(не определен)";
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

