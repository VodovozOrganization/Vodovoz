using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Chats;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Repositories;
using QS.Project.Services;
using QS.Tdi;
using QSOrmProject;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Repositories.HumanResources;
using Vodovoz.Repository.Logistics;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class RouteListKeepingDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>, ITDICloseControlTab
	{
		//2 уровня доступа к виджетам, для всех и для логистов.
		private bool allEditing = true;
		private bool logisticanEditing = true;
		private bool isUserLogist = true;
		private Employee previousForwarder = null;
		WageParameterService wageParameterService = new WageParameterService(WageSingletonRepository.GetInstance(), new BaseParametersProvider());

		public event RowActivatedHandler OnClosingItemActivated;

		public RouteListKeepingDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = string.Format("Ведение МЛ №{0}", Entity.Id);
			allEditing = Entity.Status == RouteListStatus.EnRoute;
			isUserLogist = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
			logisticanEditing = isUserLogist && allEditing;

			ConfigureDlg();
		}

		public RouteListKeepingDlg(RouteList sub) : this(sub.Id) { }

		public RouteListKeepingDlg(int routeId, int[] selectOrderId) : this(routeId)
		{
			var selectedItems = items.Where(x => selectOrderId.Contains(x.RouteListItem.Order.Id)).ToArray();
			if(selectedItems.Any()) {
				ytreeviewAddresses.SelectObject(selectedItems);
				var iter = ytreeviewAddresses.YTreeModel.IterFromNode(selectedItems[0]);
				var path = ytreeviewAddresses.YTreeModel.GetPath(iter);
				ytreeviewAddresses.ScrollToCell(path, ytreeviewAddresses.Columns[0], true, 0.5f, 0.5f);
			}
		}

		public override bool HasChanges {
			get {
				if(items.All(x => x.Status != RouteListItemStatus.EnRoute))
					return true; //Хак, чтобы вылезало уведомление о закрытии маршрутного листа, даже если ничего не меняли.
				return base.HasChanges;
			}
		}

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						OrderSingletonRepository.GetInstance(),
						EmployeeSingletonRepository.GetInstance(),
						new BaseParametersProvider(),
						ServicesConfig.CommonServices.UserService,
						SingletonErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		Dictionary<RouteListItemStatus, Gdk.Pixbuf> statusIcons = new Dictionary<RouteListItemStatus, Gdk.Pixbuf>();

		List<RouteListKeepingItemNode> items;
		RouteListKeepingItemNode selectedItem;

		public void ConfigureDlg()
		{
			Entity.ObservableAddresses.ElementAdded += ObservableAddresses_ElementAdded;
			Entity.ObservableAddresses.ElementRemoved += ObservableAddresses_ElementRemoved;
			Entity.ObservableAddresses.ElementChanged += ObservableAddresses_ElementChanged; ;

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(
				new DefaultEntityAutocompleteSelectorFactory<Car, CarJournalViewModel, CarJournalFilterViewModel>(ServicesConfig.CommonServices));
			entityviewmodelentryCar.Binding.AddBinding(Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);
			entityviewmodelentryCar.Sensitive = logisticanEditing;

			var filterDriver = new EmployeeFilterViewModel();
			filterDriver.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking
			);
			referenceDriver.RepresentationModel = new EmployeesVM(filterDriver);
			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.Sensitive = logisticanEditing;
			var filterForwarder = new EmployeeFilterViewModel();
			filterForwarder.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.forwarder,
				x => x.Status = EmployeeStatus.IsWorking
			);
			referenceForwarder.RepresentationModel = new EmployeesVM(filterForwarder);
			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.Sensitive = logisticanEditing;
			referenceForwarder.Changed += ReferenceForwarder_Changed;

			referenceLogistican.RepresentationModel = new EmployeesVM();
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.Sensitive = logisticanEditing;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = logisticanEditing;

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = logisticanEditing;

			ylabelLastTimeCall.Binding.AddFuncBinding(Entity, e => GetLastCallTime(e.LastCallTime), w => w.LabelProp).InitializeFromSource();
			yspinActualDistance.Sensitive = allEditing;

			buttonMadeCall.Sensitive = allEditing;

			buttonRetriveEnRoute.Sensitive = Entity.Status == RouteListStatus.OnClosing && isUserLogist
				&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_retrieve_routelist_en_route");

			buttonNewFine.Sensitive = allEditing;

			buttonRefresh.Sensitive = allEditing;

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
					.AddEnumRenderer(node => node.Status, excludeItems: new Enum[] { RouteListItemStatus.Transfered })
					.AddSetter((c, n) => c.Editable = allEditing && n.Status != RouteListItemStatus.Transfered)
				.AddColumn("Отгрузка")
					.AddNumericRenderer(node => node.RouteListItem.Order.OrderItems
					.Where(b => b.Nomenclature.Category == NomenclatureCategory.water && b.Nomenclature.TareVolume == TareVolume.Vol19L)
					.Sum(b => b.Count))
				.AddColumn("Возврат тары")
					.AddNumericRenderer(node => node.RouteListItem.Order.BottlesReturn)
				.AddColumn("Сдали по факту")
					.AddNumericRenderer(node => node.RouteListItem.DriverBottlesReturned)
				.AddColumn("Статус изменен")
					.AddTextRenderer(node => node.LastUpdate)
				.AddColumn("Комментарий")
					.AddTextRenderer(node => node.Comment)
					.Editable(allEditing)
				.AddColumn("Переносы")
					.AddTextRenderer(node => node.Transferred)
				.RowCells()
					.AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.RowColor)
				.Finish();
			ytreeviewAddresses.Selection.Mode = SelectionMode.Multiple;
			ytreeviewAddresses.Selection.Changed += OnSelectionChanged;
			ytreeviewAddresses.Sensitive = allEditing;
			ytreeviewAddresses.RowActivated += YtreeviewAddresses_RowActivated;

			//Заполняем телефоны
			string phones = null;
			if(Entity.Driver != null && Entity.Driver.Phones.Count > 0) {
				phones = string.Format("<b>Водитель {0}:</b>\n{1}",
										Entity.Driver.FullName,
										string.Join("\n", Entity.Driver.Phones));
			}
			if(Entity.Forwarder != null && Entity.Forwarder.Phones.Count > 0) {
				if(!string.IsNullOrWhiteSpace(phones))
					phones += "\n";
				phones += string.Format("<b>Экспедитор {0}:</b>\n{1}",
										 Entity.Forwarder.FullName,
										 string.Join("\n", Entity.Forwarder.Phones));
			}

			if(string.IsNullOrWhiteSpace(phones))
				phones = "Нет телефонов";
			labelPhonesInfo.Markup = phones;

			//Заполняем информацию о бутылях
			UpdateBottlesSummaryInfo();

			UpdateNodes();
		}

		void YtreeviewAddresses_RowActivated(object o, RowActivatedArgs args)
		{
			selectedItem = ytreeviewAddresses.GetSelectedObjects<RouteListKeepingItemNode>().FirstOrDefault();
			if(selectedItem != null) {
				var dlg = new OrderDlg(selectedItem.RouteListItem.Order) {
					HasChanges = false
				};
				dlg.SetDlgToReadOnly();
				OpenSlaveTab(dlg);
			}
		}

		private void UpdateBottlesSummaryInfo()
		{
			string bottles = null;
			int completedBottles = Entity.Addresses.Where(x => x != null && x.Status == RouteListItemStatus.Completed).Sum(x => x.Order.TotalWaterBottles);
			int canceledBottles = Entity.Addresses.Where(
				  x => x != null && (x.Status == RouteListItemStatus.Canceled
					|| x.Status == RouteListItemStatus.Overdue
					|| x.Status == RouteListItemStatus.Transfered)
				).Sum(x => x.Order.TotalWaterBottles);
			int enrouteBottles = Entity.Addresses.Where(x => x != null && x.Status == RouteListItemStatus.EnRoute).Sum(x => x.Order.TotalWaterBottles);
			bottles = string.Format("<b>Всего 19л. бутылей в МЛ:</b>\n");
			bottles += string.Format("Выполнено: <b>{0}</b>\n", completedBottles);
			bottles += string.Format(" Отменено: <b>{0}</b>\n", canceledBottles);
			bottles += string.Format(" Осталось: <b>{0}</b>\n", enrouteBottles);
			labelBottleInfo.Markup = bottles;
		}

		void ObservableAddresses_ElementAdded(object aList, int[] aIdx)
		{
			UpdateBottlesSummaryInfo();
		}

		void ObservableAddresses_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			UpdateBottlesSummaryInfo();
		}

		void ObservableAddresses_ElementChanged(object aList, int[] aIdx)
		{
			UpdateBottlesSummaryInfo();
		}

		public string GetLastCallTime(DateTime? lastCall)
		{
			if(lastCall == null)
				return "Водителю еще не звонили.";
			if(lastCall.Value.Date == Entity.Date)
				return string.Format("Последний звонок был в {0:t}", lastCall);
			return string.Format("Последний звонок был {0:g}", lastCall);
		}

		public void UpdateNodes()
		{
			List<string> emptyDP = new List<string>();
			items = new List<RouteListKeepingItemNode>();
			foreach(var item in Entity.Addresses.Where(x => x != null)) {
				items.Add(new RouteListKeepingItemNode { RouteListItem = item });
				if(item.Order.DeliveryPoint == null) {
					emptyDP.Add(string.Format(
						"Для заказа {0} не определена точка доставки.",
						item.Order.Id));
				}
			}
			if(emptyDP.Any()) {
				string message = string.Join(Environment.NewLine, emptyDP);
				message += Environment.NewLine + "Необходимо добавить точки доставки или сохранить вышеуказанные заказы снова.";
				MessageDialogHelper.RunErrorDialog(message);
				FailInitialize = true;
				return;
			}
			items.ForEach(i => i.StatusChanged += RLI_StatusChanged);

			ytreeviewAddresses.ItemsDataSource = new GenericObservableList<RouteListKeepingItemNode>(items);
		}

		void RLI_StatusChanged(object sender, StatusChangedEventArgs e)
		{
			var newStatus = e.NewStatus;
			if(sender is RouteListKeepingItemNode rli) {
				if(newStatus == RouteListItemStatus.Canceled || newStatus == RouteListItemStatus.Overdue) {
					UndeliveryOnOrderCloseDlg dlg = new UndeliveryOnOrderCloseDlg(rli.RouteListItem.Order, rli.RouteListItem.RouteList.UoW);
					TabParent.AddSlaveTab(this, dlg);
					dlg.DlgSaved += (s, ea) => rli.UpdateStatus(newStatus, CallTaskWorker);
					return;
				}
				rli.UpdateStatus(newStatus, CallTaskWorker);
			}
		}

		public void OnSelectionChanged(object sender, EventArgs args)
		{
			buttonSetStatusComplete.Sensitive = ytreeviewAddresses.GetSelectedObjects().Any() && allEditing;
			buttonChangeDeliveryTime.Sensitive = ytreeviewAddresses.GetSelectedObjects().Count() == 1 
													&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistic_changedeliverytime")
													&& allEditing;								
		}

		void ReferenceForwarder_Changed(object sender, EventArgs e)
		{
			var newForwarder = Entity.Forwarder;

			if(Entity.Status == RouteListStatus.OnClosing
				&& ((previousForwarder == null && newForwarder != null)
					|| (previousForwarder != null && newForwarder == null)))
				Entity.RecalculateAllWages(wageParameterService);

			previousForwarder = Entity.Forwarder;
		}

		#region implemented abstract members of OrmGtkDialogBase

		private bool canClose = true;
		public bool CanClose()
		{
			if(!canClose)
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения работы задачи и повторите");
			return canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			canClose = isSensetive;
			buttonSave.Sensitive = isSensetive;
			buttonCancel.Sensitive = isSensetive;
		}

		public override bool Save()
		{
			try {
				SetSensetivity(false);
				if(Entity.Status == RouteListStatus.EnRoute && items.All(x => x.Status != RouteListItemStatus.EnRoute)) {
					if(MessageDialogHelper.RunQuestionDialog("В маршрутном листе не осталось адресов со статусом в 'В пути'. Завершить маршрут?")) {
						Entity.CompleteRoute(wageParameterService, CallTaskWorker);
					}
				}

				UoWGeneric.Save();

				var changedList = items.Where(item => item.ChangedDeliverySchedule || item.HasChanged).ToList();
				if(changedList.Count == 0)
					return true;

				var currentEmployee = EmployeeRepository.GetEmployeeForCurrentUser(UoWGeneric);
				if(currentEmployee == null) {
					MessageDialogHelper.RunInfoDialog("Ваш пользователь не привязан к сотруднику, уведомления об изменениях в маршрутном листе не будут отправлены водителю.");
					return true;
				}

				foreach(var item in changedList) {
					if(item.HasChanged)
						GetChatService()
							.SendOrderStatusNotificationToDriver(
								currentEmployee.Id,
								item.RouteListItem.Id
							);
					if(item.ChangedDeliverySchedule)
						GetChatService()
							.SendDeliveryScheduleNotificationToDriver(
								currentEmployee.Id,
								item.RouteListItem.Id
							);
				}
				return true;
			} finally {
				SetSensetivity(true);
			}
		}

		#endregion

		static IChatService GetChatService()
		{
			return new ChannelFactory<IChatService>(
				new BasicHttpBinding(),
				"http://driver.vod.qsolution.ru:7071/ChatService"
			).CreateChannel();
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			bool hasChanges = items.Any(item => item.HasChanged);
			if(!hasChanges || MessageDialogHelper.RunQuestionDialog("Вы действительно хотите обновить список заказов? Внесенные изменения будут утрачены.")) {
				UoWGeneric.Session.Refresh(Entity);
				UpdateNodes();
			}
		}

		protected void OnButtonChangeDeliveryTimeClicked(object sender, EventArgs e)
		{
			if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistic_changedeliverytime")) {
				var selectedObjects = ytreeviewAddresses.GetSelectedObjects();
				if(selectedObjects.Count() != 1)
					return;
				var selectedAddress = selectedObjects
					.Cast<RouteListKeepingItemNode>()
					.FirstOrDefault();

				OrmReference SelectDialog;

				ICriteria ItemsCriteria = UoWGeneric.Session.CreateCriteria(typeof(DeliverySchedule));
				SelectDialog = new OrmReference(typeof(DeliverySchedule), UoWGeneric, ItemsCriteria) {
					Mode = OrmReferenceMode.Select
				};
				SelectDialog.ObjectSelected += (selectSender, selectE) => {
					if(selectedAddress.RouteListItem.Order.DeliverySchedule != (DeliverySchedule)selectE.Subject) {
						selectedAddress.RouteListItem.Order.DeliverySchedule = (DeliverySchedule)selectE.Subject;
						selectedAddress.ChangedDeliverySchedule = true;
					}
				};
				TabParent.AddSlaveTab(this, SelectDialog);
			}
		}

		protected void OnButtonSetStatusCompleteClicked(object sender, EventArgs e)
		{
			var selectedObjects = ytreeviewAddresses.GetSelectedObjects();
			foreach(RouteListKeepingItemNode item in selectedObjects) {
				if(item.Status == RouteListItemStatus.Transfered)
					continue;
				item.RouteListItem.UpdateStatus(UoW, RouteListItemStatus.Completed, CallTaskWorker);
			}
		}

		protected void OnButtonNewFineClicked(object sender, EventArgs e)
		{
			this.TabParent.AddSlaveTab(
				this,
				new FineDlg(default(decimal), Entity)
			);
		}

		protected void OnButtonMadeCallClicked(object sender, EventArgs e)
		{
			Entity.LastCallTime = DateTime.Now;
		}

		protected void OnButtonRetriveEnRouteClicked(object sender, EventArgs e)
		{
			Entity.RollBackEnRouteStatus();
		}
	}

	public class RouteListKeepingItemNode : PropertyChangedBase
	{
		public bool HasChanged = false;
		public bool ChangedDeliverySchedule = false;
		public event EventHandler<StatusChangedEventArgs> StatusChanged;

		public Gdk.Color RowColor {
			get {
				switch(RouteListItem.Status) {
					case RouteListItemStatus.Overdue:
						return new Gdk.Color(0xee, 0x66, 0x66);
					case RouteListItemStatus.Completed:
						return new Gdk.Color(0x66, 0xee, 0x66);
					case RouteListItemStatus.Canceled:
						return new Gdk.Color(0xaf, 0xaf, 0xaf);
					default:
						return new Gdk.Color(0xff, 0xff, 0xff);
				}
			}
		}

		RouteListItemStatus status;
		public RouteListItemStatus Status {
			get => RouteListItem.Status;
			set {
				status = value;
				StatusChanged?.Invoke(this, new StatusChangedEventArgs(value));
			}
		}

		public string Comment {
			get => RouteListItem.Comment;
			set {
				RouteListItem.Comment = value;
				OnPropertyChanged<string>(() => Comment);
			}
		}

		public string LastUpdate {
			get {
				var maybeLastUpdate = RouteListItem.StatusLastUpdate;
				if(maybeLastUpdate.HasValue) {
					if(maybeLastUpdate.Value.Date == DateTime.Today) {
						return maybeLastUpdate.Value.ToShortTimeString();
					} else
						return maybeLastUpdate.Value.ToString();
				}
				return string.Empty;
			}
		}

		public string Transferred => RouteListItem.GetTransferText(RouteListItem);

		RouteListItem routeListItem;

		public RouteListItem RouteListItem {
			get => routeListItem;
			set {
				routeListItem = value;
				if(RouteListItem != null)
					RouteListItem.PropertyChanged += (sender, e) => OnPropertyChanged(() => RouteListItem);
			}
		}

		public void UpdateStatus(RouteListItemStatus value, CallTaskWorker callTaskWorker)
		{
			var uow = RouteListItem.RouteList.UoW;
			RouteListItem.UpdateStatus(uow, value, callTaskWorker);
			HasChanged = true;
			OnPropertyChanged<RouteListItemStatus>(() => Status);
		}
	}

	public class StatusChangedEventArgs : EventArgs
	{
		public RouteListItemStatus NewStatus { get; private set; }
		public StatusChangedEventArgs(RouteListItemStatus newStatus) => NewStatus = newStatus;
	}
}

