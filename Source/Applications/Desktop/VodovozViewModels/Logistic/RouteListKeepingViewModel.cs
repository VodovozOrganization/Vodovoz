using Autofac;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Factories;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Roboats;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz
{
	public partial class RouteListKeepingViewModel : EntityDialogViewModelBase<RouteList>, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		private ILifetimeScope _lifetimeScope;
		private IEmployeeRepository _employeeRepository;
		private IDeliveryShiftRepository _deliveryShiftRepository;
		private IRouteListProfitabilityController _routeListProfitabilityController;
		private IWageParameterService _wageParameterService;
		private IGeneralSettings _generalSettingsSettings;
		private readonly bool _isOrderWaitUntilActive;

		//2 уровня доступа к виджетам, для всех и для логистов.
		private readonly bool _allEditing;
		private readonly bool _logisticanEditing;
		private readonly bool _isUserLogist;
		private Employee _previousForwarder = null;

		private DeliveryFreeBalanceViewModel _deliveryFreeBalanceViewModel;
		private readonly Dictionary<RouteListItemStatus, Gdk.Pixbuf> _statusIcons = new Dictionary<RouteListItemStatus, Gdk.Pixbuf>();
		private List<RouteListKeepingItemNode> _items;
		private RouteListKeepingItemNode _selectedItem;
		private bool _canClose = true;

		public event RowActivatedHandler OnClosingItemActivated;

		public RouteListKeepingDlg(int id)
		{
			ResolveDependencies();
			Build();

			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = $"Ведение МЛ №{Entity.Id}";
			_allEditing = Entity.Status == RouteListStatus.EnRoute && permissionResult.CanUpdate;
			_isUserLogist = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Permissions.Logistic.IsLogistician);
			_logisticanEditing = _isUserLogist && _allEditing;

			_isOrderWaitUntilActive = _generalSettingsSettings.GetIsOrderWaitUntilActive;

			ConfigureDlg();
		}

		public RouteListKeepingDlg(RouteList sub) : this(sub.Id) { }

		public RouteListKeepingDlg(int routeId, int[] selectOrderId) : this(routeId)
		{
			var selectedItems = _items.Where(x => selectOrderId.Contains(x.RouteListItem.Order.Id)).ToArray();

			if(selectedItems.Any())
			{
				ytreeviewAddresses.SelectObject(selectedItems);
				var iter = ytreeviewAddresses.YTreeModel.IterFromNode(selectedItems[0]);
				var path = ytreeviewAddresses.YTreeModel.GetPath(iter);
				ytreeviewAddresses.ScrollToCell(path, ytreeviewAddresses.Columns[0], true, 0.5f, 0.5f);
			}
		}

		public ITdiCompatibilityNavigation NavigationManager { get; } = Startup.MainWin.NavigationManager;

		public override bool HasChanges
		{
			get
			{
				if(_items.All(x => x.Status != RouteListItemStatus.EnRoute))
				{
					return true; //Хак, чтобы вылезало уведомление о закрытии маршрутного листа, даже если ничего не меняли.
				}

				return base.HasChanges;
			}
		}

		public bool AskSaveOnClose => permissionResult.CanUpdate;

		public virtual ICallTaskWorker CallTaskWorker { get; private set; }

		public void ResolveDependencies()
		{
			_lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();

			_employeeRepository = _lifetimeScope.Resolve<IEmployeeRepository>();
			_deliveryShiftRepository = _lifetimeScope.Resolve<IDeliveryShiftRepository>();
			_routeListProfitabilityController = _lifetimeScope.Resolve<IRouteListProfitabilityController>();
			_wageParameterService = _lifetimeScope.Resolve<IWageParameterService>();
			_generalSettingsSettings = _lifetimeScope.Resolve<IGeneralSettings>();

			CallTaskWorker = _lifetimeScope.Resolve<ICallTaskWorker>();
		}

		private void ConfigureDlg()
		{
			buttonSave.Sensitive = _allEditing;

			Entity.ObservableAddresses.ElementAdded += ObservableAddresses_ElementAdded;
			Entity.ObservableAddresses.ElementRemoved += ObservableAddresses_ElementRemoved;
			Entity.ObservableAddresses.ElementChanged += ObservableAddresses_ElementChanged;

			entityentryCar.ViewModel = BuildCarEntryViewModel();
			entityentryCar.Sensitive = _logisticanEditing;

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
			var driverFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, driverFilter);
			evmeDriver.SetEntityAutocompleteSelectorFactory(driverFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			evmeDriver.Sensitive = _logisticanEditing;
			evmeDriver.Changed += OnEvmeDriverChanged;

			var forwarderFilter = new EmployeeFilterViewModel();
			forwarderFilter.SetAndRefilterAtOnce(
				x => x.Status = EmployeeStatus.IsWorking,
				x => x.RestrictCategory = EmployeeCategory.forwarder);
			var forwarderFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager, forwarderFilter);
			evmeForwarder.SetEntityAutocompleteSelectorFactory(forwarderFactory.CreateEmployeeAutocompleteSelectorFactory());
			evmeForwarder.Binding.AddSource(Entity)
				.AddBinding(rl => rl.Forwarder, widget => widget.Subject)
				.AddFuncBinding(rl => _logisticanEditing && rl.CanAddForwarder, widget => widget.Sensitive)
				.InitializeFromSource();

			evmeForwarder.Changed += ReferenceForwarder_Changed;

			var employeeFactory = new EmployeeJournalFactory(Startup.MainWin.NavigationManager);
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
			var ass = Assembly.GetAssembly(typeof(Startup));
			_statusIcons.Add(RouteListItemStatus.EnRoute, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.car.png"));
			_statusIcons.Add(RouteListItemStatus.Completed, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-smile-grin.png"));
			_statusIcons.Add(RouteListItemStatus.Overdue, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-angry.png"));
			_statusIcons.Add(RouteListItemStatus.Canceled, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-crying.png"));
			_statusIcons.Add(RouteListItemStatus.Transfered, new Gdk.Pixbuf(ass, "Vodovoz.icons.status.face-uncertain.png"));

			ytreeviewAddresses.ColumnsConfig = ColumnsConfigFactory.Create<RouteListKeepingItemNode>()
				.AddColumn("№ п/п").AddNumericRenderer(x => x.RouteListItem.IndexInRoute + 1)
				.AddColumn("Заказ")
					.AddTextRenderer(node => node.RouteListItem.Order.Id.ToString())
				.AddColumn("Адрес")
					.AddTextRenderer(node => node.RouteListItem.Order.DeliveryPoint == null ? "Требуется точка доставки" : node.RouteListItem.Order.DeliveryPoint.ShortAddress)
				.AddColumn("Ожидает до")
					.AddTimeRenderer(node => node.WaitUntil)
					.AddSetter((c, n) => c.Editable = _isOrderWaitUntilActive && n.RouteListItem.Order.OrderStatus == Domain.Orders.OrderStatus.OnTheWay)
					.WidthChars(5)
				.AddColumn("Время")
					.AddTextRenderer(node => node.RouteListItem.Order.DeliverySchedule == null ? "" : node.RouteListItem.Order.DeliverySchedule.Name)
				.AddColumn("Статус")
					.AddPixbufRenderer(x => _statusIcons[x.Status])
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

			if(Entity.Driver != null && Entity.Driver.Phones.Count > 0)
			{
				uint rows = Convert.ToUInt32(Entity.Driver.Phones.Count + 1);
				PhonesTable1.Resize(rows, 2);
				Label label = new Label();
				label.LabelProp = $"{Entity.Driver.FullName}";
				PhonesTable1.Attach(label, 0, 2, 0, 1);

				for(uint i = 1; i < rows; i++)
				{
					Label l = new Label();
					l.LabelProp = "+7 " + Entity.Driver.Phones[Convert.ToInt32(i - 1)].Number;
					l.Selectable = true;
					PhonesTable1.Attach(l, 0, 1, i, i + 1);

					HandsetView h = new HandsetView(Entity.Driver.Phones[Convert.ToInt32(i - 1)].DigitsNumber);
					PhonesTable1.Attach(h, 1, 2, i, i + 1);
				}
			}

			if(Entity.Forwarder != null && Entity.Forwarder.Phones.Count > 0)
			{
				uint rows = Convert.ToUInt32(Entity.Forwarder.Phones.Count + 1);
				PhonesTable2.Resize(rows, 2);
				Label label = new Label();
				label.LabelProp = $"{Entity.Forwarder.FullName}";
				PhonesTable2.Attach(label, 0, 2, 0, 1);

				for(uint i = 1; i < rows; i++)
				{
					Label l = new Label();
					l.LabelProp = "+7 " + Entity.Forwarder.Phones[Convert.ToInt32(i - 1)].Number;
					l.Selectable = true;
					PhonesTable2.Attach(l, 0, 1, i, i + 1);

					HandsetView h = new HandsetView(Entity.Forwarder.Phones[Convert.ToInt32(i - 1)].DigitsNumber);
					PhonesTable2.Attach(h, 1, 2, i, i + 1);
				}
			}

			//Телефон
			PhonesTable1.ShowAll();
			PhonesTable2.ShowAll();

			phoneLogistican.MangoManager = phoneDriver.MangoManager = phoneForwarder.MangoManager = Startup.MainWin.MangoManager;
			phoneLogistican.Binding.AddBinding(Entity, e => e.Logistician, w => w.Employee).InitializeFromSource();
			phoneDriver.Binding.AddBinding(Entity, e => e.Driver, w => w.Employee).InitializeFromSource();
			phoneForwarder.Binding.AddBinding(Entity, e => e.Forwarder, w => w.Employee).InitializeFromSource();

			//Заполняем информацию о бутылях
			UpdateBottlesSummaryInfo();

			UpdateNodes();

			btnCopyEntityId.Clicked += OnBtnCopyEntityIdClicked;
		}

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var viewModel = new LegacyEEVMBuilderFactory<RouteList>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Car)
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
					filter =>
					{
					})
				.UseViewModelDialog<CarViewModel>()
				.Finish();

			viewModel.CanViewEntity = ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			return viewModel;
		}

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(Entity.Id > 0)
			{
				GetClipboard(Gdk.Selection.Clipboard).Text = Entity.Id.ToString();
			}
		}

		private void OnEvmeDriverChanged(object sender, EventArgs e)
		{
			if(Entity.Driver != null)
			{
				if(!Entity.IsDriversDebtInPermittedRangeVerification())
				{
					Entity.Driver = null;
				}
			}
		}

		private void YtreeviewAddresses_RowActivated(object o, RowActivatedArgs args)
		{
			_selectedItem = ytreeviewAddresses.GetSelectedObjects<RouteListKeepingItemNode>().FirstOrDefault();

			if(_selectedItem != null)
			{
				var dlg = new OrderDlg(_selectedItem.RouteListItem.Order)
				{
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

			bottles = "<b>Всего 19л. бутылей в МЛ:</b>\n";
			bottles += $"Выполнено: <b>{completedBottles}</b>\n";
			bottles += $" Отменено: <b>{canceledBottles}</b>\n";
			bottles += $" Осталось: <b>{enrouteBottles}</b>\n";
			labelBottleInfo.Markup = bottles;
		}

		private void ObservableAddresses_ElementAdded(object aList, int[] aIdx)
		{
			UpdateBottlesSummaryInfo();
		}

		private void ObservableAddresses_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			UpdateBottlesSummaryInfo();
		}

		private void ObservableAddresses_ElementChanged(object aList, int[] aIdx)
		{
			UpdateBottlesSummaryInfo();
		}

		public string GetLastCallTime(DateTime? lastCall)
		{
			if(lastCall == null)
			{
				return "Водителю еще не звонили.";
			}

			if(lastCall.Value.Date == Entity.Date)
			{
				return $"Последний звонок был в {lastCall:t}";
			}

			return $"Последний звонок был {lastCall:g}";
		}

		public void UpdateNodes()
		{
			var emptyDP = new List<string>();
			_items = new List<RouteListKeepingItemNode>();

			foreach(var item in Entity.Addresses.Where(x => x != null))
			{
				_items.Add(new RouteListKeepingItemNode { RouteListItem = item });

				if(item.Order.DeliveryPoint == null)
				{
					emptyDP.Add($"Для заказа {item.Order.Id} не определена точка доставки.");
				}
			}

			if(emptyDP.Any())
			{
				var message = string.Join(Environment.NewLine, emptyDP);
				message += Environment.NewLine + "Необходимо добавить точки доставки или сохранить вышеуказанные заказы снова.";
				MessageDialogHelper.RunErrorDialog(message);
				FailInitialize = true;
				return;
			}

			_items.ForEach(i => i.StatusChanged += RLI_StatusChanged);

			ytreeviewAddresses.ItemsDataSource = new GenericObservableList<RouteListKeepingItemNode>(_items);
		}

		private void RLI_StatusChanged(object sender, StatusChangedEventArgs e)
		{
			var newStatus = e.NewStatus;

			if(sender is RouteListKeepingItemNode rli)
			{
				if(newStatus == RouteListItemStatus.Canceled || newStatus == RouteListItemStatus.Overdue)
				{
					UndeliveryOnOrderCloseDlg dlg = new UndeliveryOnOrderCloseDlg(rli.RouteListItem.Order, rli.RouteListItem.RouteList.UoW);
					TabParent.AddSlaveTab(this, dlg);

					dlg.DlgSaved += (s, ea) =>
					{
						rli.UpdateStatus(newStatus, CallTaskWorker);
						UoW.Save(rli.RouteListItem);
						UoW.Commit();
					};

					return;
				}

				var uowFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();

				var validationContext = new ValidationContext(Entity, null, new Dictionary<object, object>
				{
					{ "uowFactory", uowFactory }
				});

				var canCreateSeveralOrdersValidationResult =
					rli.RouteListItem.Order.ValidateCanCreateSeveralOrderForDateAndDeliveryPoint(validationContext);

				if(canCreateSeveralOrdersValidationResult != ValidationResult.Success)
				{
					ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Нельзя перевести адрес в статус \"{newStatus.GetEnumTitle()}\": {canCreateSeveralOrdersValidationResult.ErrorMessage} ");

					return;
				}

				rli.UpdateStatus(newStatus, CallTaskWorker);
			}
		}

		public void OnSelectionChanged(object sender, EventArgs args)
		{
			buttonSetStatusComplete.Sensitive = ytreeviewAddresses.GetSelectedObjects().Any() && _allEditing;
			buttonChangeDeliveryTime.Sensitive =
				ytreeviewAddresses.GetSelectedObjects().Count() == 1
				&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistic_changedeliverytime")
				&& _allEditing;
		}

		private void ReferenceForwarder_Changed(object sender, EventArgs e)
		{
			var newForwarder = Entity.Forwarder;

			if(Entity.Status == RouteListStatus.OnClosing
				&& ((_previousForwarder == null && newForwarder != null)
					|| (_previousForwarder != null && newForwarder == null)))
			{
				Entity.RecalculateAllWages(_wageParameterService);
			}

			_previousForwarder = Entity.Forwarder;
		}

		#region implemented abstract members of OrmGtkDialogBase


		public bool CanClose()
		{
			if(!_canClose)
			{
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения работы задачи и повторите");
			}

			return _canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			_canClose = isSensetive;
			buttonSave.Sensitive = isSensetive;
			buttonCancel.Sensitive = isSensetive;
		}

		public override bool Save()
		{
			try
			{
				SetSensetivity(false);

				Entity.CalculateWages(_wageParameterService);

				UoWGeneric.Save();

				_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
				UoW.Save(Entity.RouteListProfitability);
				UoW.Commit();

				var changedList = _items.Where(item => item.ChangedDeliverySchedule || item.HasChanged).ToList();

				if(changedList.Count == 0)
				{
					return true;
				}

				var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoWGeneric);

				if(currentEmployee == null)
				{
					MessageDialogHelper.RunInfoDialog("Ваш пользователь не привязан к сотруднику, уведомления об изменениях в маршрутном листе не будут отправлены водителю.");
					return true;
				}

				return true;
			}
			finally
			{
				SetSensetivity(true);
			}
		}

		#endregion

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			bool hasChanges = _items.Any(item => item.HasChanged);

			if(!hasChanges || MessageDialogHelper.RunQuestionDialog("Вы действительно хотите обновить список заказов? Внесенные изменения будут утрачены."))
			{
				UoWGeneric.Session.Refresh(Entity);
				UpdateNodes();
			}
		}

		protected void OnButtonChangeDeliveryTimeClicked(object sender, EventArgs e)
		{
			if(ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistic_changedeliverytime"))
			{
				var selectedObjects = ytreeviewAddresses.GetSelectedObjects();

				if(selectedObjects.Count() != 1)
				{
					return;
				}

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
						ServicesConfig.UnitOfWorkFactory, ServicesConfig.CommonServices, deliveryScheduleRepository, roboatsViewModelFactory);
				journal.SelectionMode = JournalSelectionMode.Single;
				journal.OnEntitySelectedResult += (s, args) =>
				{
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
			var page = NavigationManager.OpenViewModelOnTdi<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);

			page.ViewModel.SetRouteListById(Entity.Id);
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
}
