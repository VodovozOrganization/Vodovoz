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
using Vodovoz;
using Vodovoz.Controllers;
using Vodovoz.Core;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.Parameters;
using Vodovoz.SidePanel;
using VodovozInfrastructure.Configuration;
using Order = Vodovoz.Domain.Orders.Order;
using ToolbarStyle = Vodovoz.Domain.Employees.ToolbarStyle;

public partial class MainWindow : Gtk.Window
{
	private static Logger _logger = LogManager.GetCurrentClassLogger();
	private uint _lastUiId;
	private readonly ILifetimeScope _autofacScope = Startup.AppDIContainer.BeginLifetimeScope();
	private readonly IApplicationInfo _applicationInfo;
	private readonly IPasswordValidator _passwordValidator;
	private readonly IApplicationConfigurator _applicationConfigurator;
	private readonly IMovementDocumentsNotificationsController _movementsNotificationsController;
	private readonly IComplaintNotificationController _complaintNotificationController;
	private readonly bool _hasAccessToSalariesForLogistics;
	private readonly int _currentUserSubdivisionId;
	private readonly IEnumerable<int> _curentUserMovementDocumentsNotificationWarehouses;
	private readonly bool _hideComplaintsNotifications;

	private bool _accessOnlyToWarehouseAndComplaints;

	public TdiNotebook TdiMain => tdiMain;
	public InfoPanel InfoPanel => infopanel;

	public MainWindow(IPasswordValidator passwordValidator, IApplicationConfigurator applicationConfigurator) : base(Gtk.WindowType.Toplevel)
	{
		_passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
		_applicationConfigurator = applicationConfigurator ?? throw new ArgumentNullException(nameof(applicationConfigurator));
		
		Build();

		PerformanceHelper.AddTimePoint("Закончена стандартная сборка окна.");
		_applicationInfo = new ApplicationVersionInfo();

		BuildToolbarActions();

		tdiMain.WidgetResolver = ViewModelWidgetResolver.Instance;
		TDIMain.MainNotebook = tdiMain;
		var highlightWColor = CurrentUserSettings.Settings.HighlightTabsWithColor;
		var keepTabColor = CurrentUserSettings.Settings.KeepTabColor;
		var reorderTabs = CurrentUserSettings.Settings.ReorderTabs;
		_hideComplaintsNotifications = CurrentUserSettings.Settings.HideComplaintNotification;
		var tabsParametersProvider = new TabsParametersProvider(new ParametersProvider());
		TDIMain.SetTabsColorHighlighting(highlightWColor, keepTabColor, GetTabsColors(), tabsParametersProvider.TabsPrefix);
		TDIMain.SetTabsReordering(reorderTabs);

		if(reorderTabs)
		{
			ReorderTabs.Activate();
		}

		if(highlightWColor)
		{
			HighlightTabsWithColor.Activate();
		}

		if(keepTabColor)
		{
			KeepTabColor.Activate();
		}

		bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

		if(isWindows)
		{
			KeyPressEvent += HotKeyHandler.HandleKeyPressEvent;
		}

		Title = $"{_applicationInfo.ProductTitle} v{_applicationInfo.Version.Major}.{_applicationInfo.Version.Minor} от {GetDateTimeFGromVersion(_applicationInfo.Version):dd.MM.yyyy HH:mm}";

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

		//Читаем настройки пользователя
		switch(CurrentUserSettings.Settings.ToolbarStyle)
		{
			case ToolbarStyle.Both:
				ActionToolBarBoth.Activate();
				break;
			case ToolbarStyle.Icons:
				ActionToolBarIcon.Activate();
				break;
			case ToolbarStyle.Text:
				ActionToolBarText.Activate();
				break;
		}

		switch(CurrentUserSettings.Settings.ToolBarIconsSize)
		{
			case IconsSize.ExtraSmall:
				ActionIconsExtraSmall.Activate();
				break;
			case IconsSize.Small:
				ActionIconsSmall.Activate();
				break;
			case IconsSize.Middle:
				ActionIconsMiddle.Activate();
				break;
			case IconsSize.Large:
				ActionIconsLarge.Activate();
				break;
		}

		// Отдел продаж

		ActionSalesDepartment.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_sales_department");

		#region Пользователь с правом работы только со складом и рекламациями

		using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
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

		using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			_currentUserSubdivisionId = GetEmployeeSubdivisionId(uow);
			_curentUserMovementDocumentsNotificationWarehouses = CurrentUserSettings.Settings.MovementDocumentsNotificationUserSelectedWarehouses;
			_movementsNotificationsController = _autofacScope
				.Resolve<IMovementDocumentsNotificationsController>(
					new TypedParameter(typeof(int), _currentUserSubdivisionId), 
					new TypedParameter(typeof(IEnumerable<int>), _curentUserMovementDocumentsNotificationWarehouses));

			var notificationDetails = _movementsNotificationsController.GetNotificationDetails(uow);

			var message = notificationDetails.SendedMovementsCount > 0 ? $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">{notificationDetails.NotificationMessage}</span>": notificationDetails.NotificationMessage;

			hboxMovementsNotification.Visible = notificationDetails.NeedNotify;
			lblMovementsNotification.Markup = message;

			if(notificationDetails.NeedNotify)
			{
				_movementsNotificationsController.UpdateNotificationAction += UpdateSendedMovementsNotification;
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

		using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
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

		ActionGroupPricing.Activated += ActionGroupPricingActivated;
		ActionProfitabilitySalesReport.Activated += ActionProfitabilitySalesReportActivated;

		Action74.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Cash.CanGenerateCashFlowDdsReport);

		InitializeThemesMenuItem();
	}
	
	public TdiNavigationManager NavigationManager { get; private set; }
	public MangoManager MangoManager { get; private set; }

	/// <summary>
	/// Пока в <see cref="EmployeeJournalFactory"/> есть получение <see cref="NavigationManager"/> через <see cref="MainWindow"/>
	/// то инициализируем менеджеры после инициализации главного окна
	/// </summary>
	public void InitializeManagers()
	{
		NavigationManager = _autofacScope.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), tdiMain));
		MangoManager = _autofacScope.Resolve<MangoManager>(new TypedParameter(typeof(Gtk.Action), MangoAction));
		MangoManager.Connect();
	}

	private DateTime GetDateTimeFGromVersion(Version version) =>
		new DateTime(2000, 1, 1)
			.AddDays(version.Build)
			.AddSeconds(version.Revision * 2);
}
