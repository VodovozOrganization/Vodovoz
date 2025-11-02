using Autofac;
using MassTransit;
using NLog;
using Pacs.Core;
using QS.Commands;
using QS.Dialog;
using QS.Navigation;
using QS.Project.Versioning;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Utilities.Debug;
using QS.Validation;
using QSBanks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Gtk;
using QS.Project.Services;
using QSProjectsLib;
using Vodovoz;
using Vodovoz.Application.Pacs;
using Vodovoz.Controllers;
using Vodovoz.Core;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Presentation.ViewModels.Pacs;
using Vodovoz.Services;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Tabs;
using Vodovoz.SidePanel;
using Vodovoz.ViewModels.Dialogs.Mango;
using VodovozInfrastructure.Configuration;
using Order = Vodovoz.Domain.Orders.Order;
using ToolbarStyle = Vodovoz.Core.Domain.Users.Settings.ToolbarStyle;

public partial class MainWindow : Gtk.Window
{
	private static Logger _logger = LogManager.GetCurrentClassLogger();
	private uint _lastUiId;
	private readonly ILifetimeScope _autofacScope = Startup.AppDIContainer.BeginLifetimeScope();
	private readonly IApplicationInfo _applicationInfo;
	private readonly IInteractiveService _interativeService;
	private readonly IPasswordValidator _passwordValidator;
	private readonly IApplicationConfigurator _applicationConfigurator;
	private readonly IWikiSettings _wikiSettings;
	private readonly IMovementDocumentsNotificationsController _movementsNotificationsController;
	private readonly IComplaintNotificationController _complaintNotificationController;
	private readonly bool _hasAccessToSalariesForLogistics;
	private readonly int _currentUserSubdivisionId;
	private readonly IEnumerable<int> _curentUserMovementDocumentsNotificationWarehouses;
	private readonly bool _hideComplaintsNotifications;
	private readonly OperatorService _operatorService;
	private bool _accessOnlyToWarehouseAndComplaints;
	private IBusControl _messageBusControl;

	public TdiNotebook TdiMain => tdiMain;
	public InfoPanel InfoPanel => infopanel;

	public MainWindow(IPasswordValidator passwordValidator, IApplicationConfigurator applicationConfigurator, IWikiSettings wikiSettings) : base(Gtk.WindowType.Toplevel)
	{
		_passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
		ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
		_applicationConfigurator = applicationConfigurator ?? throw new ArgumentNullException(nameof(applicationConfigurator));
		_wikiSettings = wikiSettings ?? throw new ArgumentNullException(nameof(wikiSettings));
		Build();

		TdiMain = tdiMain;
		InfoPanel = infopanel;
		ToolbarMain = toolbarMain;
		ToolbarComplaints = tlbComplaints;

		PerformanceHelper.AddTimePoint("Закончена стандартная сборка окна.");
	}
	
	public void Configure()
	{
		BuildToolbarActions();
		ConfigureMainMenu();

		tdiMain.WidgetResolver = ViewModelWidgetResolver.Instance;
		TDIMain.MainNotebook = tdiMain;
		_hideComplaintsNotifications = CurrentUserSettings.Settings.HideComplaintNotification;
		var tabsParametersProvider = new TabsParametersProvider(new ParametersProvider());
		TDIMain.SetTabsColorHighlighting(
			CurrentUserSettings.Settings.HighlightTabsWithColor,
			CurrentUserSettings.Settings.KeepTabColor,
			GetTabsColors(),
			tabsParametersProvider.TabsPrefix);
		TDIMain.SetTabsReordering(CurrentUserSettings.Settings.ReorderTabs);

		bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		if(isWindows)
		{
			KeyPressEvent += HotKeyHandler.HandleKeyPressEvent;
		}

		Title = $"{ApplicationInfo.ProductTitle} v{ApplicationInfo.Version.Major}.{ApplicationInfo.Version.Minor} от {GetDateTimeFGromVersion(ApplicationInfo.Version):dd.MM.yyyy HH:mm}";

		//Настраиваем модули
		var admin = QSMain.User.Admin;

		pacspanelview1.ViewModel = _autofacScope.Resolve<PacsPanelViewModel>();

		ActionUsers.Sensitive = QSMain.User.Admin;
		ActionAdministration.Sensitive = QSMain.User.Admin;
		labelUser.LabelProp = QSMain.User.Name;
		var commonServices = ServicesConfig.CommonServices;
		var cashier = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.PresetPermissionsRoles.Cashier);
		ActionCash.Sensitive = ActionIncomeBalanceReport.Sensitive = ActionCashBook.Sensitive = cashier;
		ActionAccounting.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("money_manage_bookkeeping");
		Action1SWork.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.BookkeeppingPermissions.Work1S.HasAccessTo1sWork);
		ActionRouteListsAtDay.Sensitive =
			ActionRouteListTracking.Sensitive =
			ActionRouteListMileageCheck.Sensitive =
			ActionRouteListAddressesTransferring.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.IsLogistician);
		var currentWarehousePermissions = new CurrentWarehousePermissions();
		ActionStock.Sensitive = currentWarehousePermissions.WarehousePermissions.Any(x => x.PermissionValue == true);

		var hasAccessToCRM = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_crm");
		ActionCRM.Sensitive = hasAccessToCRM;

		bool canEditWage = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.EmployeePermissions.CanEditWage);
		ActionWageDistrict.Sensitive = canEditWage;
		ActionRates.Sensitive = canEditWage;

		bool canEditWageBySelfSubdivision =
			commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_wage_by_self_subdivision");
		ActionSalesPlans.Sensitive = canEditWageBySelfSubdivision;

		ActionFinesJournal.Visible = ActionPremiumJournal.Visible =
			commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_fines_bonuses");
		ActionReports.Sensitive = false;
		//ActionServices.Visible = false;
		//ActionService.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("database_maintenance");
		ActionEmployeeWorkChart.Sensitive = false;

		ActionAddOrder.Sensitive = commonServices.PermissionService.ValidateUserPermission(typeof(Order), QSMain.User.Id)?.CanCreate ?? false;
		ActionExportImportNomenclatureCatalog.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");
		ActionDistricts.Sensitive = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DistrictsSet)).CanRead;
		ActionDriversStopLists.Sensitive = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DriverStopListRemoval)).CanRead;

		// Отдел продаж

		ActionSalesDepartment.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_sales_department");

		#region Пользователь с правом работы только со складом и рекламациями

		using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
		{
			_accessOnlyToWarehouseAndComplaints =
				commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !commonServices.UserService.GetCurrentUser().IsAdmin;
		}

		MainMenuBar.Visible = ActionOrders.Visible = ActionServices.Visible = ActionLogistics.Visible = ActionCash.Visible =
			ActionAccounting.Visible = ActionReports.Visible = ActionArchive.Visible = ActionStaff.Visible = ActionCRM.Visible =
				ActionSuppliers.Visible = ActionCashRequest.Visible = ActionRetail.Visible = ActionCarService.Visible =
					/*MangoAction.Visible =*/ !_accessOnlyToWarehouseAndComplaints;

		#endregion

		#region Уведомление об отправленных перемещениях для подразделения

		using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
		{
			var employeeService = _autofacScope.Resolve<IEmployeeService>();

			_currentUserSubdivisionId = employeeService.GetEmployeeForCurrentUser()?.Subdivision?.Id ?? 0;
			_curentUserMovementDocumentsNotificationWarehouses = CurrentUserSettings.Settings.MovementDocumentsNotificationUserSelectedWarehouses;
			_movementsNotificationsController = _autofacScope
				.Resolve<IMovementDocumentsNotificationsController>(
					new TypedParameter(typeof(int), _currentUserSubdivisionId),
					new TypedParameter(typeof(IEnumerable<int>), _curentUserMovementDocumentsNotificationWarehouses));

			var notificationDetails = _movementsNotificationsController.GetNotificationDetails(uow);
			UpdateSentMovementsNotification(notificationDetails.Notification);
			hboxMovementsNotification.Visible = notificationDetails.NeedNotify;

			if(notificationDetails.NeedNotify)
			{
				_movementsNotificationsController.UpdateNotificationAction += UpdateSentMovementsNotification;
			}
		}

		btnUpdateNotifications.Clicked += OnBtnUpdateNotificationClicked;

		#endregion

		#region Уведомление о наличии незакрытых рекламаций без комментариев в добавленной дискуссии для отдела

		_complaintNotificationController = _autofacScope.Resolve<IComplaintNotificationController>(new TypedParameter(typeof(int), _currentUserSubdivisionId));

		if(!_hideComplaintsNotifications)
		{
			_complaintNotificationController.UpdateNotificationAction += UpdateSendedComplaintsNotification;

			var complaintNotificationDetails = GetComplaintNotificationDetails();
			UpdateSendedComplaintsNotification(complaintNotificationDetails);

			btnOpenComplaint.Clicked += OnBtnOpenComplaintClicked;
		}
		else
		{
			hboxComplaintsNotification.Visible = false;
		}
		#endregion

		hboxNotifications.Visible = hboxMovementsNotification.Visible || !_hideComplaintsNotifications;

		BanksUpdater.CheckBanksUpdate(false);

		// Блокировка отчетов для торговых представителей

		bool userIsSalesRepresentative;

		using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
		{
			userIsSalesRepresentative = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.UserPermissions.IsSalesRepresentative)
				&& !commonServices.UserService.GetCurrentUser().IsAdmin;
		}

		// Основные разделы отчетов

		ActionReportOrders.Visible =
			ActionReportsStock.Visible =
			ActionOSKOKKReports.Visible =
			ActionLogistic.Visible =
			ActionReportEmployees.Visible =
			ActionReportsDrivers.Visible =
			ActionReportService.Visible =
			ActionBookkeepping.Visible =
			ActionCashMenubar.Visible = // Касса
			ActionRetailMenubar.Visible =
			ActionTransportMenuBar.Visible =
			ActionProduction.Visible = !userIsSalesRepresentative;// Производство

		// Отчеты в Продажи

		ActionOrderCreationDateReport.Visible =
			ActionPlanImplementationReport.Visible =
			ActionSetBillsReport.Visible = !userIsSalesRepresentative;

		// Управление ограничением доступа через зарегистрированные RM

		var userCanManageRegisteredRMs = commonServices.CurrentPermissionService.ValidatePresetPermission("user_can_manage_registered_rms");

		registeredRMAction.Visible = userCanManageRegisteredRMs;

		// Настройки розницы

		var userHaveAccessToRetail = commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");

		ActionRetail.Sensitive = userHaveAccessToRetail;
		ActionRetailUndeliveredOrdersJournal.Sensitive = false; // Этот журнал не готов - выключено до реализации фичи
	}

	private void ConfigureMainMenu()
	{
		var menuCreator = _autofacScope.Resolve<MainMenuBarCreator>();
		MainMenuBar = menuCreator.CreateMenuBar();
		vboxMain.Add(MainMenuBar);

		//Доступ к константам рентабельности (Справочники - Финансы - Константы рентабельности)
		ProfitabilityConstantsAction.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission("can_read_and_edit_profitability_constants");

		ExternalCounterpartiesMatchingAction.Label = "Сопоставление клиентов из внешних источников";
		ExternalCounterpartiesMatchingAction.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission("can_matching_counterparties_from_external_sources");

		ActionGroupPricing.Activated += ActionGroupPricingActivated;
		ActionProfitabilitySalesReport.Activated += ActionProfitabilitySalesReportActivated;

		Action74.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CashPermissions.CanGenerateCashFlowDdsReport);

		ActionClassificationCalculation.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CounterpartyPermissions.CanCalculateCounterpartyClassifications);

		ActionInnerPhones.Activated += OnInnerPhonesActionActivated;
		CarOwnershipReportAction.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.HasAccessToCarOwnershipReport);

		InitializeThemesMenuItem();

		OpenWikiCommand = new DelegateCommand(OpenWiki);
		ybuttonWiki.BindCommand(OpenWikiCommand);

		this.KeyPressEvent += OnKeyPressed;
		var box = (Box.BoxChild)vboxMain[MainMenuBar];
		box.Position = 0;
		box.Expand = false;
	}

	public IApplicationInfo ApplicationInfo { get; }
	public TdiNotebook TdiMain { get; }
	public InfoPanel InfoPanel { get; }
	public Toolbar ToolbarMain { get; }
	public Toolbar ToolbarComplaints { get; }
	public MenuBar MainMenuBar { get; private set; }
	

	private void OnKeyPressed(object o, Gtk.KeyPressEventArgs args)
	{
		if(args.Event.Key == Gdk.Key.F1)
		{
			OpenWikiCommand.Execute();
		}
	}

	public ITdiCompatibilityNavigation NavigationManager { get; private set; }
	public MangoManager MangoManager { get; private set; }
	
	private string[] GetTabsColors() =>
		new[] { "#F81919", "#009F6B", "#1F8BFF", "#FF9F00", "#FA7A7A", "#B46034", "#99B6FF", "#8F2BE1", "#00CC44" };

	/// <summary>
	/// Пока в <see cref="EmployeeJournalFactory"/> есть получение <see cref="NavigationManager"/> через <see cref="MainWindow"/>
	/// то инициализируем менеджеры после инициализации главного окна
	/// </summary>
	public void InitializeManagers()
	{
		NavigationManager = _autofacScope.Resolve<ITdiCompatibilityNavigation>(new TypedParameter(typeof(TdiNotebook), tdiMain));
		MangoManager = _autofacScope.Resolve<MangoManager>();
	}

	private DateTime GetDateTimeFGromVersion(Version version) =>
		new DateTime(2000, 1, 1)
			.AddDays(version.Build)
			.AddSeconds(version.Revision * 2);

	public DelegateCommand OpenWikiCommand { get; }

	private void OpenWiki()
	{
		Process.Start(new ProcessStartInfo(_wikiSettings.Url) { UseShellExecute = true });
	}

	protected override void OnDestroyed()
	{
		/*if(_messageBusControl != null)
        {
            _messageBusControl.Start();
        }*/

		base.OnDestroyed();
	}

	protected void OnServiceDeliveryRulesActivated(object sender, EventArgs e)
	{
	}
}
