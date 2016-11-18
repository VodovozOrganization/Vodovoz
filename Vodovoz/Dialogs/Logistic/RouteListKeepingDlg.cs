using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;
using Vodovoz.Domain.Employees;
using System.Reflection;
using Chat;
using System.ServiceModel;
using Vodovoz.Repository;
using NHibernate;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListKeepingDlg : OrmGtkDialogBase<RouteList>
	{		
		public RouteListKeepingDlg(int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = String.Format("Ведение маршрутного листа №{0}",Entity.Id);
			ConfigureDlg ();
		}

		public RouteListKeepingDlg(RouteList sub) : this(sub.Id){ }

		public override bool HasChanges
		{
			get
			{
				if (items.All(x => x.Status != RouteListItemStatus.EnRoute))
					return true; //Хак, чтобы вылезало уведомление о закрытии маршрутного листа, даже если ничего не меняли.
				return base.HasChanges;
			}
		}

		Dictionary<RouteListItemStatus, Gdk.Pixbuf> statusIcons = new Dictionary<RouteListItemStatus, Gdk.Pixbuf>();

		List<RouteListKeepingItemNode> items;

		public void ConfigureDlg(){
			referenceCar.Binding.AddBinding(Entity, rl => rl.Car, widget => widget.Subject).InitializeFromSource();
			referenceCar.Sensitive = false;

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceDriver.Sensitive = false;

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Sensitive = false;

			referenceLogistican.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery();
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceLogistican.Sensitive = false;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = false;

			yspinPlannedDistance.Binding.AddBinding(Entity, rl => rl.PlannedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinPlannedDistance.Sensitive = false;

			yspinActualDistance.Binding.AddBinding(Entity, rl => rl.ActualDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinActualDistance.Sensitive = false;

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = false;

			//Заполняем иконки
			var ass = Assembly.GetAssembly(typeof(MainClass));
			statusIcons.Add(RouteListItemStatus.EnRoute, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.car.png"));
			statusIcons.Add(RouteListItemStatus.Completed, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-smile-grin.png"));
			statusIcons.Add(RouteListItemStatus.Overdue, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-angry.png"));
			statusIcons.Add(RouteListItemStatus.Canceled, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-crying.png"));

			ytreeviewAddresses.ColumnsConfig = ColumnsConfigFactory.Create<RouteListKeepingItemNode>()
				.AddColumn("Заказ")
					.AddTextRenderer(node => node.RouteListItem.Order.Id.ToString())					
				.AddColumn("Адрес")
					.AddTextRenderer(node => node.RouteListItem.Order.DeliveryPoint.ShortAddress)					
				.AddColumn("Время")
					.AddTextRenderer(node => node.RouteListItem.Order.DeliverySchedule == null ? "" : node.RouteListItem.Order.DeliverySchedule.Name)					
				.AddColumn("Статус")
					.AddPixbufRenderer(x => statusIcons[x.Status])
					.AddEnumRenderer(node => node.Status).Editing(true)					
				.AddColumn("Отгрузка")
				.AddNumericRenderer(node => node.RouteListItem.Order.OrderItems
					.Where(b => 
						b.Nomenclature.Category == Vodovoz.Domain.Goods.NomenclatureCategory.water ||
						b.Nomenclature.Category == Vodovoz.Domain.Goods.NomenclatureCategory.disposableBottleWater)
					.Sum(b => b.Count))
				.AddColumn("Возврат тары")
					.AddNumericRenderer(node => node.RouteListItem.Order.BottlesReturn)
				.AddColumn("Сдали по факту")
					.AddNumericRenderer(node => node.RouteListItem.DriverBottlesReturned)
				.AddColumn("Последнее изменение")
					.AddTextRenderer(node => node.LastUpdate)
				.AddColumn("Комментарий")
					.AddTextRenderer(node => node.Comment)
						.Editable(true)
				.RowCells ()
					.AddSetter<CellRenderer> ((cell, node) => cell.CellBackgroundGdk = node.RowColor)
				.Finish();
			ytreeviewAddresses.Selection.Mode = SelectionMode.Multiple;
			ytreeviewAddresses.Selection.Changed += OnSelectionChanged;
			UpdateNodes();
		}

		public void UpdateNodes(){
			items = new List<RouteListKeepingItemNode>();
			foreach (var item in Entity.Addresses)
				items.Add(new RouteListKeepingItemNode{ RouteListItem = item });
			ytreeviewAddresses.ItemsDataSource = items;
		}

		public void OnSelectionChanged(object sender, EventArgs args){
			buttonNewRouteList.Sensitive = ytreeviewAddresses.GetSelectedObjects().Count() > 0;
			buttonChangeDeliveryTime.Sensitive = ytreeviewAddresses.GetSelectedObjects().Count() == 1;
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			foreach (var address in items.Where(item=>item.HasChanged).Select(item=>item.RouteListItem))
			{
				switch (address.Status)
				{
					case RouteListItemStatus.Canceled:
						address.Order.ChangeStatus(Vodovoz.Domain.Orders.OrderStatus.DeliveryCanceled);
						break;
					case RouteListItemStatus.Completed:
						address.Order.ChangeStatus(Vodovoz.Domain.Orders.OrderStatus.Shipped);
						break;
					case RouteListItemStatus.EnRoute:
						address.Order.ChangeStatus(Vodovoz.Domain.Orders.OrderStatus.OnTheWay);
						break;
					case RouteListItemStatus.Overdue:
						address.Order.ChangeStatus(Vodovoz.Domain.Orders.OrderStatus.NotDelivered);
						break;
				}
				UoWGeneric.Save(address.Order);
			}

			if (items.All(x => x.Status != RouteListItemStatus.EnRoute))
			{
				if(MessageDialogWorks.RunQuestionDialog("В маршрутном листе не осталось адресов со статусом в 'В пути'. Завершить маршрут?"))
				{
					Entity.CompleteRoute();
				}
			}

			UoWGeneric.Save();

			foreach (var item in items.Where(item=>item.ChangedDeliverySchedule || item.HasChanged))
			{
				if (item.HasChanged)
					getChatService().SendOrderStatusNotificationToDriver(
						EmployeeRepository.GetEmployeeForCurrentUser(UoWGeneric).Id,
						item.RouteListItem.Id
					);
				if (item.ChangedDeliverySchedule)
					getChatService().SendDeliveryScheduleNotificationToDriver(
						EmployeeRepository.GetEmployeeForCurrentUser(UoWGeneric).Id,
					item.RouteListItem.Id
					);
			}
			return true;
		}
		#endregion

		protected void OnButtonNewRouteListClicked (object sender, EventArgs e)
		{
			if (TabParent.CheckClosingSlaveTabs(this))
				return;
			if (UoWGeneric.HasChanges && CommonDialogs.SaveBeforeCreateSlaveEntity(EntityObject.GetType(), typeof(RouteList)))
			{
				if (!Save())
					return;
			}
			var selectedObjects = ytreeviewAddresses.GetSelectedObjects();
			var selectedAddreses = selectedObjects
				.Cast<RouteListKeepingItemNode>()
				.Select(item=>item.RouteListItem)
				.Where(item=>item.Status==RouteListItemStatus.EnRoute);

			var dlg = new RouteListCreateDlg(Entity, selectedAddreses);
			dlg.EntitySaved += OnNewRouteListCreated;
			TabParent.AddSlaveTab(this,dlg);
		}

		public void OnNewRouteListCreated(object sender, EntitySavedEventArgs args){
			var newRouteList = args.Entity as RouteList;
			foreach (var address in newRouteList.Addresses)
			{
				var transferedAddress = Entity.ObservableAddresses.FirstOrDefault(item => item.Order.Id == address.Order.Id);
				if (transferedAddress != null)
					Entity.RemoveAddress(transferedAddress);
			}
			UpdateNodes();
			Save();
		}

		static IChatService getChatService()
		{
			return new ChannelFactory<IChatService>(
				new BasicHttpBinding(), 
				"http://vod-srv.qsolution.ru:9000/ChatService").CreateChannel();
		}

		protected void OnButtonRefreshClicked (object sender, EventArgs e)
		{
			bool hasChanges = items.Count(item => item.HasChanged) > 0;
			if (!hasChanges || MessageDialogWorks.RunQuestionDialog("Вы действительно хотите обновить список заказов? Внесенные изменения будут утрачены."))
			{
				UoWGeneric.Session.Refresh(Entity);
				UpdateNodes();
			}
		}

		protected void OnButtonChangeDeliveryTimeClicked (object sender, EventArgs e)
		{
			var selectedObjects = ytreeviewAddresses.GetSelectedObjects();
			if (selectedObjects.Count() != 1)
				return;
			var selectedAddress = selectedObjects
				.Cast<RouteListKeepingItemNode>()
				.FirstOrDefault();


			OrmReference SelectDialog;

			ICriteria ItemsCriteria = UoWGeneric.Session.CreateCriteria (typeof(DeliverySchedule));
			SelectDialog = new OrmReference (typeof(DeliverySchedule), UoWGeneric, ItemsCriteria);

			SelectDialog.Mode = OrmReferenceMode.Select;
			SelectDialog.ObjectSelected += (selectSender, selectE) => {
				if (selectedAddress.RouteListItem.Order.DeliverySchedule != (DeliverySchedule)selectE.Subject) 
				{
					selectedAddress.RouteListItem.Order.DeliverySchedule = (DeliverySchedule)selectE.Subject;
					selectedAddress.ChangedDeliverySchedule = true;
				}
			};
			TabParent.AddSlaveTab (this, SelectDialog);
		}
	}	

	public class RouteListKeepingItemNode : PropertyChangedBase
	{
		public bool HasChanged = false;
		public bool ChangedDeliverySchedule = false;

		public Gdk.Color RowColor{
			get{
				switch (RouteListItem.Status){						
					case RouteListItemStatus.Overdue:							
						return new Gdk.Color(0xee,0x66,0x66);
					case RouteListItemStatus.Completed:
						return new Gdk.Color(0x66,0xee,0x66);
					case RouteListItemStatus.Canceled:
						return new Gdk.Color(0xaf,0xaf,0xaf);
					default:
						return new Gdk.Color(0xff,0xff,0xff);
				}
			}
		}

		public RouteListItemStatus Status{
			get{
				return RouteListItem.Status;
			}
			set{
				RouteListItem.UpdateStatus(value);
				HasChanged = true;
				OnPropertyChanged<RouteListItemStatus>(() => Status);
			}
		}

		public string Comment{
			get { return RouteListItem.Comment; }
			set{
				RouteListItem.Comment = value;
				OnPropertyChanged<string>(() => Comment);
			}
		}

		public string LastUpdate {
			get{
				var maybeLastUpdate = RouteListItem.StatusLastUpdate;
				if (maybeLastUpdate.HasValue)
				{
					if (maybeLastUpdate.Value.Date == DateTime.Today)
					{
						return maybeLastUpdate.Value.ToShortTimeString();
					}
					else
						return maybeLastUpdate.Value.ToString();
				}
				else
				{
					return "";
				}
			}
		}

		public RouteListItem RouteListItem{get;set;}
	}
}

