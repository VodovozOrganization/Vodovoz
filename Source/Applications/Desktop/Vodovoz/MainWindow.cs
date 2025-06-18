using Autofac;
using MassTransit;
using NLog;
using Pacs.Core;
using QS.Commands;
using QS.Dialog;
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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Vodovoz;
using Vodovoz.Application.Pacs;
using Vodovoz.Controllers;
using Vodovoz.Core;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
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
		_applicationConfigurator = applicationConfigurator ?? throw new ArgumentNullException(nameof(applicationConfigurator));
		_wikiSettings = wikiSettings ?? throw new ArgumentNullException(nameof(wikiSettings));
		Build();

		_interativeService = ServicesConfig.CommonServices.InteractiveService;
		var transportInitializer = _autofacScope.Resolve<IMessageTransportInitializer>();
		transportInitializer.Initialize();

		var pacsEndpointConnector = _autofacScope.Resolve<PacsEndpointsConnector>();
		pacsEndpointConnector.ConnectPacsEndpoints();

		PerformanceHelper.AddTimePoint("Закончена стандартная сборка окна.");
		_applicationInfo = new ApplicationVersionInfo();

		BuildToolbarActions();

		tdiMain.WidgetResolver = ViewModelWidgetResolver.Instance;
		TDIMain.MainNotebook = tdiMain;
		_operatorService = _autofacScope.Resolve<OperatorService>();

		var highlightWColor = CurrentUserSettings.Settings.HighlightTabsWithColor;
		var keepTabColor = CurrentUserSettings.Settings.KeepTabColor;
		var reorderTabs = CurrentUserSettings.Settings.ReorderTabs;
		_hideComplaintsNotifications = CurrentUserSettings.Settings.HideComplaintNotification;
		var tabsSettings = _autofacScope.Resolve<ITabsSettings>();
		TDIMain.SetTabsColorHighlighting(highlightWColor, keepTabColor, GetTabsColors(), tabsSettings.TabsPrefix);
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

		pacspanelview1.ViewModel = _autofacScope.Resolve<PacsPanelViewModel>();

		ActionUsers.Sensitive = QSMain.User.Admin;
		ActionAdministration.Sensitive = QSMain.User.Admin;
		labelUser.LabelProp = QSMain.User.Name;
		var commonServices = ServicesConfig.CommonServices;
		var cashier = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.Cash.PresetPermissionsRoles.Cashier);
		ActionCash.Sensitive = ActionIncomeBalanceReport.Sensitive = ActionCashBook.Sensitive = cashier;
		ActionAccounting.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("money_manage_bookkeeping");
		Action1SWork.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.Bookkeepping.Work1S.HasAccessTo1sWork);
		ActionRouteListsAtDay.Sensitive =
			ActionRouteListTracking.Sensitive =
			ActionRouteListMileageCheck.Sensitive =
			ActionRouteListAddressesTransferring.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.Logistic.IsLogistician);
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

		bool canEditWage = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.Employee.CanEditWage);
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

		using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
		{
			_accessOnlyToWarehouseAndComplaints =
				commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.User.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !commonServices.UserService.GetCurrentUser().IsAdmin;
		}

		menubarMain.Visible = ActionOrders.Visible = ActionServices.Visible = ActionLogistics.Visible = ActionCash.Visible =
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
			userIsSalesRepresentative = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.User.IsSalesRepresentative)
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

		Action74.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.Cash.CanGenerateCashFlowDdsReport);

		ActionClassificationCalculation.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.Counterparty.CanCalculateCounterpartyClassifications);

		ActionInnerPhones.Activated += OnInnerPhonesActionActivated;
		CarOwnershipReportAction.Sensitive =
			commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.Logistic.Car.HasAccessToCarOwnershipReport);

		InitializeThemesMenuItem();

		OpenWikiCommand = new DelegateCommand(OpenWiki);
		ybuttonWiki.BindCommand(OpenWikiCommand);

		this.KeyPressEvent += OnKeyPressed;
	}

	private void OnKeyPressed(object o, Gtk.KeyPressEventArgs args)
	{
		if(args.Event.Key == Gdk.Key.F1)
		{
			OpenWikiCommand.Execute();
		}
	}

	public ITdiCompatibilityNavigation NavigationManager { get; private set; }
	public MangoManager MangoManager { get; private set; }

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
