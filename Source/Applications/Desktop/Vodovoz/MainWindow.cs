using Autofac;
using NLog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services;
using QS.Project.Versioning;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Utilities.Debug;
using QS.Validation;
using QSBanks;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Gtk;
using Vodovoz;
using Vodovoz.Controllers;
using Vodovoz.Core;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.MainMenu;
using Vodovoz.Parameters;
using Vodovoz.SidePanel;
using Vodovoz.TempAdapters;
using VodovozInfrastructure.Configuration;
using Order = Vodovoz.Domain.Orders.Order;

public partial class MainWindow : Gtk.Window
{
	private static Logger _logger = LogManager.GetCurrentClassLogger();
	private uint _lastUiId;
	private readonly ILifetimeScope _autofacScope = Startup.AppDIContainer.BeginLifetimeScope();
	private readonly IPasswordValidator _passwordValidator;
	private readonly IApplicationConfigurator _applicationConfigurator;
	private IMovementDocumentsNotificationsController _movementsNotificationsController;
	private IComplaintNotificationController _complaintNotificationController;
	private bool _hasAccessToSalariesForLogistics;
	private int _currentUserSubdivisionId;
	private IEnumerable<int> _curentUserMovementDocumentsNotificationWarehouses;
	private bool _hideComplaintsNotifications;

	private bool _accessOnlyToWarehouseAndComplaints;

	public MainWindow(
		IPasswordValidator passwordValidator,
		IApplicationInfo applicationInfo,
		IApplicationConfigurator applicationConfigurator) : base(Gtk.WindowType.Toplevel)
	{
		_passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
		ApplicationInfo = applicationInfo ?? throw new ArgumentNullException(nameof(applicationInfo));
		_applicationConfigurator = applicationConfigurator ?? throw new ArgumentNullException(nameof(applicationConfigurator));

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
		
		var menuCreator = _autofacScope.Resolve<MainMenuBarCreator>();
		var menu = menuCreator.CreateMenuBar();
		vboxMain.Add(menu);
		
		bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		if (isWindows)
		{
			KeyPressEvent += HotKeyHandler.HandleKeyPressEvent;
		}

		Title = $"{ApplicationInfo.ProductTitle} v{ApplicationInfo.Version.Major}.{ApplicationInfo.Version.Minor} от {GetDateTimeFGromVersion(ApplicationInfo.Version):dd.MM.yyyy HH:mm}";

		//Настраиваем модули
		ActionUsers.Sensitive = QSMain.User.Admin;
		ActionAdministration.Sensitive = QSMain.User.Admin;
		labelUser.LabelProp = QSMain.User.Name;
		var commonServices = ServicesConfig.CommonServices;
		var cashier = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.RoleCashier);
		ActionCash.Sensitive = ActionIncomeBalanceReport.Sensitive = ActionCashBook.Sensitive = cashier;
		ActionAccounting.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("money_manage_bookkeeping");
		ActionRouteListsAtDay.Sensitive =
			ActionRouteListTracking.Sensitive =
			ActionRouteListMileageCheck.Sensitive =
			ActionRouteListAddressesTransferring.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
		var currentWarehousePermissions = new CurrentWarehousePermissions();
		ActionStock.Sensitive = currentWarehousePermissions.WarehousePermissions.Any(x => x.PermissionValue == true);

		bool hasAccessToCRM = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_crm");
		bool hasAccessToSalaries = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_salaries");
		_hasAccessToSalariesForLogistics =
			commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_salary_reports_for_logistics");
		bool hasAccessToWagesAndBonuses = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_fines_bonuses");
		ActionEmployeesBonuses.Sensitive = hasAccessToWagesAndBonuses; //Премии сотрудников
		ActionEmployeeFines.Sensitive = hasAccessToWagesAndBonuses; //Штрафы сотрудников
		ActionDriverWages.Sensitive = hasAccessToSalaries; //Зарплаты водителей
		ActionWagesOperations.Sensitive = hasAccessToSalaries || _hasAccessToSalariesForLogistics; //Зарплаты сотрудников
		ActionForwarderWageReport.Sensitive = hasAccessToSalaries; //Зарплаты экспедиторов
		ActionDriversWageBalance.Visible = hasAccessToSalaries; //Баланс водителей
		EmployeesTaxesAction.Sensitive = hasAccessToSalaries; //Налоги сотрудников
		ActionCRM.Sensitive = hasAccessToCRM;

		bool canEditWage = commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_wage");
		ActionWageDistrict.Sensitive = canEditWage;
		ActionRates.Sensitive = canEditWage;

		bool canEditWageBySelfSubdivision =
			commonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_wage_by_self_subdivision");
		ActionSalesPlans.Sensitive = canEditWageBySelfSubdivision;

		ActionFinesJournal.Visible = ActionPremiumJournal.Visible =
			commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_fines_bonuses");
		ActionReports.Sensitive = false;
		//ActionServices.Visible = false;
		ActionDocTemplates.Visible = QSMain.User.Admin;
		ActionService.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("database_maintenance");
		ActionEmployeeWorkChart.Sensitive = false;

		//Скрываем справочник стажеров
		ActionTrainee.Visible = false;

		ActionAddOrder.Sensitive = commonServices.PermissionService.ValidateUserPermission(typeof(Order), QSMain.User.Id)?.CanCreate ?? false;
		ActionExportImportNomenclatureCatalog.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");
		ActionDistricts.Sensitive = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DistrictsSet)).CanRead;
		ActionDriversStopLists.Sensitive = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DriverStopListRemoval)).CanRead;

		// Отдел продаж

		ActionSalesDepartment.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_sales_department");

		#region Пользователь с правом работы только со складом и рекламациями

		using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			_accessOnlyToWarehouseAndComplaints =
				commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !commonServices.UserService.GetCurrentUser().IsAdmin;
		}

		menubarMain.Visible = ActionOrders.Visible = ActionServices.Visible = ActionLogistics.Visible = ActionCash.Visible =
			ActionAccounting.Visible = ActionReports.Visible = ActionArchive.Visible = ActionStaff.Visible = ActionCRM.Visible =
				ActionSuppliers.Visible = ActionCashRequest.Visible = ActionRetail.Visible = ActionCarService.Visible =
					MangoAction.Visible = !_accessOnlyToWarehouseAndComplaints;

		#endregion

		#region Уведомление об отправленных перемещениях для подразделения

		using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			_currentUserSubdivisionId = GetEmployeeSubdivisionId(uow);
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

		if (!_hideComplaintsNotifications)
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

		using (var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			userIsSalesRepresentative = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.User.IsSalesRepresentative)
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

		ActionAdditionalLoadSettings.Sensitive = commonServices.CurrentPermissionService
			.ValidateEntityPermission(typeof(AdditionalLoadingNomenclatureDistribution)).CanRead;

		//Доступ к константам рентабельности (Справочники - Финансы - Константы рентабельности)
		ProfitabilityConstantsAction.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission("can_read_and_edit_profitability_constants");

		ExternalCounterpartiesMatchingAction.Label = "Сопоставление клиентов из внешних источников";
		ExternalCounterpartiesMatchingAction.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission("can_matching_counterparties_from_external_sources");

		//ActionGroupPricing.Activated += ActionGroupPricingActivated;
		//ActionProfitabilitySalesReport.Activated += ActionProfitabilitySalesReportActivated;

		Action74.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.CanGenerateCashFlowDdsReport);

		ActionClassificationCalculation.Sensitive = 
			commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Counterparty.CanCalculateCounterpartyClassifications);
	}
	
	public IApplicationInfo ApplicationInfo { get; }
	public TdiNotebook TdiMain { get; }
	public InfoPanel InfoPanel { get; }
	public Toolbar ToolbarMain { get; }
	public Toolbar ToolbarComplaints { get; }
	
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
		MangoManager = _autofacScope.Resolve<MangoManager>(new TypedParameter(typeof(Gtk.Action), MangoAction));
		MangoManager.Connect();
	}

	private DateTime GetDateTimeFGromVersion(Version version) =>
		new DateTime(2000, 1, 1)
			.AddDays(version.Build)
			.AddSeconds(version.Revision * 2);
}
