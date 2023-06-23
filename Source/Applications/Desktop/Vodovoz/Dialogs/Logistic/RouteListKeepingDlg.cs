using Autofac;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Tdi;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewWidgets.Logistics;
using Vodovoz.ViewWidgets.Mango;

namespace Vodovoz
{
	public partial class RouteListKeepingDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		private readonly ILifetimeScope _lifetimeScope = MainClass.AppDIContainer.BeginLifetimeScope();
		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private static readonly INomenclatureParametersProvider _nomenclatureParametersProvider =
			new NomenclatureParametersProvider(_parametersProvider);
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IDeliveryShiftRepository _deliveryShiftRepository = new DeliveryShiftRepository();
		private readonly IRouteListProfitabilityController _routeListProfitabilityController =
			new RouteListProfitabilityController(
				new RouteListProfitabilityFactory(),
				_nomenclatureParametersProvider,
				new ProfitabilityConstantsRepository(),
				new RouteListProfitabilityRepository(),
				new RouteListRepository(new StockRepository(), new BaseParametersProvider(_parametersProvider)),
				new NomenclatureRepository(_nomenclatureParametersProvider));

		//2 уровня доступа к виджетам, для всех и для логистов.
		private readonly bool _allEditing;
		private readonly bool _logisticanEditing;
		private readonly bool _isUserLogist;
		private Employee previousForwarder = null;
		WageParameterService wageParameterService =
			new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(_parametersProvider));

		private DeliveryFreeBalanceViewModel _deliveryFreeBalanceViewModel;

		public event RowActivatedHandler OnClosingItemActivated;

		public RouteListKeepingDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = $"Ведение МЛ №{Entity.Id}";
			_allEditing = Entity.Status == RouteListStatus.EnRoute && permissionResult.CanUpdate;
			_isUserLogist = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
			_logisticanEditing = _isUserLogist && _allEditing;

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

		public override bool HasChanges
		{
			get
			{
				if(items.All(x => x.Status != RouteListItemStatus.EnRoute))
					return true; //Хак, чтобы вылезало уведомление о закрытии маршрутного листа, даже если ничего не меняли.
				return base.HasChanges;
			}
		}

		public bool AskSaveOnClose => permissionResult.CanUpdate;

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						new OrderRepository(),
						_employeeRepository,
						new BaseParametersProvider(new ParametersProvider()),
						ServicesConfig.CommonServices.UserService,
						ErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		Dictionary<RouteListItemStatus, Gdk.Pixbuf> statusIcons = new Dictionary<RouteListItemStatus, Gdk.Pixbuf>();

		List<RouteListKeepingItemNode> items;
		RouteListKeepingItemNode selectedItem;

		private void ConfigureDlg()
		{
			buttonSave.Sensitive = _allEditing;

			Entity.ObservableAddresses.ElementAdded += ObservableAddresses_ElementAdded;
			Entity.ObservableAddresses.ElementRemoved += ObservableAddresses_ElementRemoved;
			Entity.ObservableAddresses.ElementChanged += ObservableAddresses_ElementChanged;

			entityviewmodelentryCar.SetEntityAutocompleteSelectorFactory(new CarJournalFactory(MainClass.MainWin.NavigationManager).CreateCarAutocompleteSelectorFactory());
				entityviewmodelentryCar.Binding.AddBinding(Entity, e => e.Car, w => w.Subject).InitializeFromSource();
			entityviewmodelentryCar.CompletionPopupSetWidth(false);
			entityviewmodelentryCar.Sensitive = _logisticanEditing;

			_deliveryFreeBalanceViewModel = new DeliveryFreeBalanceViewModel();
			var deliveryfreebalanceview = new DeliveryFreeBalanceView(_deliveryFreeBalanceViewModel);
			deliveryfreebalanceview.Binding
				.AddBinding(Entity, e => e.ObservableDeliveryFreeBalanceOperations, w => w.ObservableDeliveryFreeBalanceOperations)
				.InitializeFromSource();
			deliveryfreebalanceview.ShowAll();
			yhboxDeliveryFreeBalance.PackStart(deliveryfreebalanceview, true, true, 0);

			var driverFilter = new EmployeeFilterViewModel();
			driverFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.driver);
			var driverFactory = new EmployeeJournalFactory(driverFilter);
			evmeDriver.SetEntityAutocompleteSelectorFactory(driverFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			evmeDriver.Sensitive = _logisticanEditing;

			var forwarderFilter = new EmployeeFilterViewModel();
			forwarderFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.forwarder);
			var forwarderFactory = new EmployeeJournalFactory(forwarderFilter);
			evmeForwarder.SetEntityAutocompleteSelectorFactory(forwarderFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeForwarder.Binding.AddSource(Entity)
				.AddBinding(rl => rl.Forwarder, widget => widget.Subject)
				.AddFuncBinding(rl => _logisticanEditing && rl.CanAddForwarder, widget => widget.Sensitive)
				.InitializeFromSource();

			evmeForwarder.Changed += ReferenceForwarder_Changed;

			var employeeFactory = new EmployeeJournalFactory();
			evmeLogistician.SetEntityAutocompleteSelectorFactory(employeeFactory.CreateWorkingEmployeeAutocompleteSelectorFactory());
			evmeLogistician.Binding.AddBinding(Entity, rl => rl.Logistician, widget => widget.Subject).InitializeFromSource();
			evmeLogistician.Sensitive = _logisticanEditing;

			speccomboShift.ItemsList = _deliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = _logisticanEditing;

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = _logisticanEditing;

			ylabelLastTimeCall.Binding.AddFuncBinding(Entity, e => GetLastCallTime(e.LastCallTime), w => w.LabelProp).InitializeFromSource();
			yspinActualDistance.Sensitive = _allEditing;

			buttonMadeCall.Sensitive = _allEditing;

			buttonRetriveEnRoute.Sensitive = Entity.Status == RouteListStatus.OnClosing && _isUserLogist
				&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_retrieve_routelist_en_route");

			btnReDeliver.Binding.AddBinding(Entity, e => e.CanChangeStatusToDeliveredWithIgnoringAdditionalLoadingDocument, w => w.Sensitive).InitializeFromSource();

			buttonNewFine.Sensitive = _allEditing;

			buttonRefresh.Sensitive = _allEditing;

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
					.AddSetter((c, n) => c.Editable = _allEditing && n.Status != RouteListItemStatus.Transfered)
				.AddColumn("Отгрузка")
					.AddNumericRenderer(node => node.RouteListItem.Order.OrderItems
					.Where(b => b.Nomenclature.Category == NomenclatureCategory.water && b.Nomenclature.TareVolume == TareVolume.Vol19L)
					.Sum(b => b.Count))
				.AddColumn("Возврат тары")
					.AddNumericRenderer(node => node.RouteListItem.Order.BottlesReturn)
				.AddColumn("Сдали по факту")
					.AddNumericRenderer(node => node.RouteListItem.DriverBottlesReturned)
				.AddColumn("Доставка за час")
					.AddToggleRenderer(x => x.RouteListItem.Order.IsFastDelivery).Editing(false)
				.AddColumn("Статус изменен")
					.AddTextRenderer(node => node.LastUpdate)
				.AddColumn("Комментарий")
					.AddTextRenderer(node => node.Comment)
					.Editable(_allEditing)
				.AddColumn("Переносы")
					.AddTextRenderer(node => node.Transferred)
				.RowCells()
					.AddSetter<CellRenderer>((cell, node) => cell.CellBackgroundGdk = node.RowColor)
				.Finish();
			ytreeviewAddresses.Selection.Mode = SelectionMode.Multiple;
			ytreeviewAddresses.Selection.Changed += OnSelectionChanged;
			ytreeviewAddresses.Sensitive = _allEditing;
			ytreeviewAddresses.RowActivated += YtreeviewAddresses_RowActivated;

			//Point!
			//Заполняем телефоны

			if(Entity.Driver != null && Entity.Driver.Phones.Count > 0) {
				uint rows = Convert.ToUInt32(Entity.Driver.Phones.Count + 1);
				PhonesTable1.Resize(rows, 2);
				Label label = new Label();
				label.LabelProp = $"{Entity.Driver.FullName}";
				PhonesTable1.Attach(label, 0, 2, 0, 1);

				for(uint i = 1; i < rows; i++) {
					Label l = new Label();
					l.LabelProp = "+7 " + Entity.Driver.Phones[Convert.ToInt32(i-1)].Number;
					l.Selectable = true;
					PhonesTable1.Attach(l, 0, 1, i, i + 1);

					HandsetView h = new HandsetView(Entity.Driver.Phones[Convert.ToInt32(i-1)].DigitsNumber);
					PhonesTable1.Attach(h, 1, 2, i, i + 1);
				}
			}
			if(Entity.Forwarder != null && Entity.Forwarder.Phones.Count > 0) {
				uint rows = Convert.ToUInt32(Entity.Forwarder.Phones.Count + 1);
				PhonesTable2.Resize(rows, 2);
				Label label = new Label();
				label.LabelProp = $"{Entity.Forwarder.FullName}";
				PhonesTable2.Attach(label, 0, 2, 0, 1);

				for(uint i = 1; i < rows; i++) {
					Label l = new Label();
					l.LabelProp = "+7 " + Entity.Forwarder.Phones[Convert.ToInt32(i-1)].Number;
					l.Selectable = true;
					PhonesTable2.Attach(l, 0, 1, i, i + 1);

					HandsetView h = new HandsetView(Entity.Forwarder.Phones[Convert.ToInt32(i-1)].DigitsNumber);
					PhonesTable2.Attach(h, 1, 2, i, i + 1);
				}
			}

			//Телефон
			PhonesTable1.ShowAll();
			PhonesTable2.ShowAll();

			phoneLogistican.MangoManager = phoneDriver.MangoManager = phoneForwarder.MangoManager = MainClass.MainWin.MangoManager;
			phoneLogistican.Binding.AddBinding(Entity, e => e.Logistician, w => w.Employee).InitializeFromSource();
			phoneDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Employee).InitializeFromSource();
			phoneForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Employee).InitializeFromSource();

			//Заполняем информацию о бутылях
			UpdateBottlesSummaryInfo();

			UpdateNodes();

			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = Entity.Id.ToString();
			}
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
			int completedBottles = Entity.Addresses.Where(x => x != null && x.Status == RouteListItemStatus.Completed)
												   .Sum(x => x.Order.Total19LBottlesToDeliver);

			int canceledBottles = Entity.Addresses.Where(
				  x => x != null && (x.Status == RouteListItemStatus.Canceled
					|| x.Status == RouteListItemStatus.Overdue
					|| x.Status == RouteListItemStatus.Transfered)
				).Sum(x => x.Order.Total19LBottlesToDeliver);

			int enrouteBottles = Entity.Addresses.Where(x => x != null && x.Status == RouteListItemStatus.EnRoute)
												 .Sum(x => x.Order.Total19LBottlesToDeliver);

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
					dlg.DlgSaved += (s, ea) =>
					{
						rli.UpdateStatus(newStatus, CallTaskWorker);
					};
					return;
				}
				rli.UpdateStatus(newStatus, CallTaskWorker);
			}
		}

		public void OnSelectionChanged(object sender, EventArgs args)
		{
			buttonSetStatusComplete.Sensitive = ytreeviewAddresses.GetSelectedObjects().Any() && _allEditing;
			buttonChangeDeliveryTime.Sensitive = ytreeviewAddresses.GetSelectedObjects().Count() == 1
													&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistic_changedeliverytime")
													&& _allEditing;
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

				Entity.CalculateWages(wageParameterService);

				UoWGeneric.Save();

				_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
				UoW.Save(Entity.RouteListProfitability);
				UoW.Commit();
				
				var changedList = items.Where(item => item.ChangedDeliverySchedule || item.HasChanged).ToList();
				if(changedList.Count == 0)
					return true;

				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoWGeneric);
				if(currentEmployee == null) {
					MessageDialogHelper.RunInfoDialog("Ваш пользователь не привязан к сотруднику, уведомления об изменениях в маршрутном листе не будут отправлены водителю.");
					return true;
				}

				return true;
			} finally {
				SetSensetivity(true);
			}
		}

		#endregion

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

				var roboatsSettings = _lifetimeScope.Resolve<IRoboatsSettings>();
				var roboatsFileStorageFactory =
					new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, ErrorReporter.Instance);
				IDeliveryScheduleRepository deliveryScheduleRepository = new DeliveryScheduleRepository();
				IFileDialogService fileDialogService = new FileDialogService();
				var roboatsViewModelFactory =
					new RoboatsViewModelFactory(
						roboatsFileStorageFactory, fileDialogService, ServicesConfig.CommonServices.CurrentPermissionService);
				var journal =
					new DeliveryScheduleJournalViewModel(
						UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, deliveryScheduleRepository, roboatsViewModelFactory);
				journal.SelectionMode = JournalSelectionMode.Single;
				journal.OnEntitySelectedResult += (s, args) => {
					var selectedResult = args.SelectedNodes.First() as DeliveryScheduleJournalNode;
					if(selectedResult == null)
					{
						return;
					}
					var selectedEntity = UoW.GetById<DeliverySchedule>(selectedResult.Id);
					if(selectedAddress.RouteListItem.Order.DeliverySchedule.Id != selectedEntity.Id)
					{
						selectedAddress.RouteListItem.Order.DeliverySchedule = selectedEntity;
						selectedAddress.ChangedDeliverySchedule = true;
					}
				};

				TabParent.AddSlaveTab(this, journal);
			}
		}

		protected void OnButtonSetStatusCompleteClicked(object sender, EventArgs e)
		{
			var selectedObjects = ytreeviewAddresses.GetSelectedObjects();
			foreach(RouteListKeepingItemNode item in selectedObjects) 
			{
				if(item.Status == RouteListItemStatus.Transfered)
				{
					continue;
				}

				Entity.ChangeAddressStatusAndCreateTask(UoW, item.RouteListItem.Id, RouteListItemStatus.Completed, CallTaskWorker);
			}
		}

		protected void OnButtonNewFineClicked(object sender, EventArgs e)
		{
			this.TabParent.AddSlaveTab(
				this,
				new FineDlg(Entity)
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

		protected void OnBtnReDeliverClicked(object sender, EventArgs e)
		{
			Entity.UpdateStatus(isIgnoreAdditionalLoadingDocument: true);
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
			RouteListItem.RouteList.ChangeAddressStatusAndCreateTask(uow, RouteListItem.Id, value, callTaskWorker);
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
