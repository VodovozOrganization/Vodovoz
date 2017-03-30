using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Chat;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository;
using Vodovoz.Repository.Logistics;

namespace Vodovoz
{
	public partial class RouteListKeepingDlg : OrmGtkDialogBase<RouteList>
	{
		private bool editing = true;

		public RouteListKeepingDlg(int id, bool canEdit = true)
		{
			this.Build ();
			editing = canEdit;
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = String.Format("Ведение маршрутного листа №{0}",Entity.Id);
			ConfigureDlg ();
		}

		public RouteListKeepingDlg(RouteList sub) : this(sub.Id){ }

		public RouteListKeepingDlg (int routeId, int[] selectOrderId,  bool canEdit = true) : this(routeId, canEdit)
		{
			var selectedItems = items.Where (x => selectOrderId.Contains(x.RouteListItem.Order.Id)).ToArray();
			if (selectedItems.Length > 0)
			{
				ytreeviewAddresses.SelectObject (selectedItems);
				var iter = ytreeviewAddresses.YTreeModel.IterFromNode (selectedItems [0]);
				var path = ytreeviewAddresses.YTreeModel.GetPath (iter);
				ytreeviewAddresses.ScrollToCell (path, ytreeviewAddresses.Columns [0], true, 0.5f, 0.5f);
			}
		}

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
			referenceCar.Sensitive = editing;

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceDriver.Sensitive = editing;

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Sensitive = editing;

			referenceLogistican.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery();
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceLogistican.Sensitive = editing;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = editing;

			yspinActualDistance.Binding.AddBinding(Entity, rl => rl.ActualDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinActualDistance.Sensitive = editing;

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = editing;

			//Заполняем иконки
			var ass = Assembly.GetAssembly(typeof(MainClass));
			statusIcons.Add(RouteListItemStatus.EnRoute, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.car.png"));
			statusIcons.Add(RouteListItemStatus.Completed, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-smile-grin.png"));
			statusIcons.Add(RouteListItemStatus.Overdue, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-angry.png"));
			statusIcons.Add(RouteListItemStatus.Canceled, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-crying.png"));
			statusIcons.Add(RouteListItemStatus.Transfered, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-uncertain.png"));

			ytreeviewAddresses.ColumnsConfig = ColumnsConfigFactory.Create<RouteListKeepingItemNode>()
				.AddColumn("№ п/п").AddNumericRenderer(x => x.RouteListItem.IndexInRoute + 1)
				.AddColumn("Заказ")
					.AddTextRenderer(node => node.RouteListItem.Order.Id.ToString())					
				.AddColumn("Адрес")
				.AddTextRenderer(node => node.RouteListItem.Order.DeliveryPoint == null ? "Требуется точка доставки" : node.RouteListItem.Order.DeliveryPoint.ShortAddress)					
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
			ytreeviewAddresses.Sensitive = editing;

			string phones = string.Join("\n", Entity.Driver.Phones);
			if (string.IsNullOrWhiteSpace(phones))
				phones = "Нет телефонов";
			ytextviewDriverPhones.Buffer.Text = phones;
			UpdateNodes();
		}

		public void UpdateNodes(){
			List<string> emptyDP = new List<string>();
			items = new List<RouteListKeepingItemNode>();
			foreach (var item in Entity.Addresses)
			{
				items.Add(new RouteListKeepingItemNode{ RouteListItem = item });
				if (item.Order.DeliveryPoint == null)
				{
					emptyDP.Add(string.Format(
						"Для заказа {0} не определена точка доставки.",
						item.Order.Id));
				}
			}
			if(emptyDP.Count > 0){
				string message = string.Join(Environment.NewLine, emptyDP);
				message += Environment.NewLine + "Необходимо добавить точки доставки или сохранить вышеуказанные заказы снова.";
				MessageDialogWorks.RunErrorDialog(message);
				FailInitialize = true;
				return;
			}
			ytreeviewAddresses.ItemsDataSource = items;
		}

		public void OnSelectionChanged(object sender, EventArgs args){
			buttonSetStatusComplete	.Sensitive = ytreeviewAddresses.GetSelectedObjects().Count() > 0;
			buttonChangeDeliveryTime.Sensitive = ytreeviewAddresses.GetSelectedObjects().Count() == 1;
		}

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			if (items.All(x => x.Status != RouteListItemStatus.EnRoute))
			{
				if(MessageDialogWorks.RunQuestionDialog("В маршрутном листе не осталось адресов со статусом в 'В пути'. Завершить маршрут?"))
				{
					Entity.CompleteRoute();
				}
			}

			UoWGeneric.Save();

			var changedList = items.Where(item => item.ChangedDeliverySchedule || item.HasChanged).ToList();
			if (changedList.Count == 0)
				return true;

			var currentEmployee = EmployeeRepository.GetEmployeeForCurrentUser(UoWGeneric);
			if (currentEmployee == null)
			{
				MessageDialogWorks.RunInfoDialog("Ваш пользователь не привязан к сотруднику, уведомления об изменениях в маршрутном листе не будут отправлены водителю.");
				return true;
			}
				
			foreach (var item in changedList)
			{
				if (item.HasChanged)
					getChatService().SendOrderStatusNotificationToDriver(
						currentEmployee.Id,
						item.RouteListItem.Id
					);
				if (item.ChangedDeliverySchedule)
					getChatService().SendDeliveryScheduleNotificationToDriver(
						currentEmployee.Id,
					item.RouteListItem.Id
					);
			}
			return true;
		}
		#endregion

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

		protected void OnButtonSetStatusCompleteClicked (object sender, EventArgs e)
		{
			var selectedObjects = ytreeviewAddresses.GetSelectedObjects();
			foreach (RouteListKeepingItemNode item in selectedObjects)
			{
				item.RouteListItem.UpdateStatus(UoW, RouteListItemStatus.Completed);
			}
		}

		protected void OnButtonNewFineClicked (object sender, EventArgs e)
		{
			this.TabParent.AddSlaveTab(
				this, new FineDlg (default(decimal), Entity)
			);
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
				var uow = RouteListItem.RouteList.UoW;
				RouteListItem.UpdateStatus(uow, value);
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

