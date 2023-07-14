using Autofac;
using Fias.Client;
using Fias.Client.Cache;
using NLog;
using QS.BaseParameters;
using QS.BaseParameters.ViewModels;
using QS.BaseParameters.Views;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Project.Versioning;
using QS.Project.ViewModels;
using QS.Project.Views;
using QS.Report.ViewModels;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Tools;
using QS.Validation;
using QSBanks;
using QSOrmProject;
using QSProjectsLib;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Vodovoz;
using Vodovoz.Controllers;
using Vodovoz.Core;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs.OnlineStore;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.StoredResources;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bookkeeping;
using Vodovoz.ReportsParameters.Bottles;
using Vodovoz.ReportsParameters.Employees;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.ReportsParameters.Payments;
using Vodovoz.ReportsParameters.Production;
using Vodovoz.ReportsParameters.Retail;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.ReportsParameters.Store;
using Vodovoz.Representations;
using Vodovoz.ServiceDialogs;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.Services;
using Vodovoz.SidePanel;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels.Accounting;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;
using Vodovoz.ViewModels.Journals.FilterViewModels.Security;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Proposal;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.Journals.JournalViewModels.Security;
using Vodovoz.ViewModels.Journals.JournalViewModels.Users;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ReportsParameters.Cash;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport;
using Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using Vodovoz.ViewModels.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using Vodovoz.ViewModels.ViewModels.Warehouses;
using VodovozInfrastructure.Configuration;
using Order = Vodovoz.Domain.Orders.Order;
using ToolbarStyle = Vodovoz.Domain.Employees.ToolbarStyle;
using UserRepository = Vodovoz.EntityRepositories.UserRepository;

public partial class MainWindow : Gtk.Window
{
	private static Logger logger = LogManager.GetCurrentClassLogger();
	private uint lastUiId;
	private readonly ILifetimeScope autofacScope = Startup.AppDIContainer.BeginLifetimeScope();
	private readonly IApplicationInfo applicationInfo;
	private readonly IPasswordValidator passwordValidator;
	private readonly IApplicationConfigurator applicationConfigurator;
	private readonly IMovementDocumentsNotificationsController _movementsNotificationsController;
	private readonly IComplaintNotificationController _complaintNotificationController;
	private readonly bool _hasAccessToSalariesForLogistics;
	private readonly int _currentUserSubdivisionId;
	private readonly bool _hideComplaintsNotifications;

	private bool _accessOnlyToWarehouseAndComplaints;

	public TdiNotebook TdiMain => tdiMain;
	public InfoPanel InfoPanel => infopanel;

	public readonly TdiNavigationManager NavigationManager;
	public readonly MangoManager MangoManager;

	public MainWindow(IPasswordValidator passwordValidator, IApplicationConfigurator applicationConfigurator) : base(Gtk.WindowType.Toplevel)
	{
		this.passwordValidator = passwordValidator ?? throw new ArgumentNullException(nameof(passwordValidator));
		this.applicationConfigurator = applicationConfigurator ?? throw new ArgumentNullException(nameof(applicationConfigurator));
		Build();
		PerformanceHelper.AddTimePoint("Закончена стандартная сборка окна.");
		applicationInfo = new ApplicationVersionInfo();
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
			ReorderTabs.Activate();
		if(highlightWColor)
			HighlightTabsWithColor.Activate();
		if(keepTabColor)
			KeepTabColor.Activate();

		bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		if(isWindows)
			KeyPressEvent += HotKeyHandler.HandleKeyPressEvent;

		Title = $"{applicationInfo.ProductTitle} v{applicationInfo.Version.Major}.{applicationInfo.Version.Minor} от {GetDateTimeFGromVersion(applicationInfo.Version):dd.MM.yyyy HH:mm}";
		//Настраиваем модули
		ActionUsers.Sensitive = QSMain.User.Admin;
		ActionAdministration.Sensitive = QSMain.User.Admin;
		labelUser.LabelProp = QSMain.User.Name;
		var commonServices = ServicesConfig.CommonServices;
		var cashier = commonServices.CurrentPermissionService.ValidatePresetPermission("role_сashier");
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

		NavigationManager = autofacScope.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), tdiMain));
		MangoManager = autofacScope.Resolve<MangoManager>(new TypedParameter(typeof(Gtk.Action), MangoAction));
		MangoManager.Connect();

		// Отдел продаж

		ActionSalesDepartment.Sensitive = commonServices.CurrentPermissionService.ValidatePresetPermission("access_to_sales_department");

		#region Пользователь с правом работы только со складом и рекламациями

		using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			_accessOnlyToWarehouseAndComplaints =
				commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !commonServices.UserService.GetCurrentUser(uow).IsAdmin;
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
			_movementsNotificationsController = autofacScope.Resolve<IMovementDocumentsNotificationsController>(new TypedParameter(typeof(int), _currentUserSubdivisionId));

			var notificationDetails = _movementsNotificationsController.GetNotificationDetails(uow);
			hboxMovementsNotification.Visible = notificationDetails.NeedNotify;
			lblMovementsNotification.Markup = notificationDetails.NotificationMessage;

			if(notificationDetails.NeedNotify)
			{
				_movementsNotificationsController.UpdateNotificationAction += UpdateSendedMovementsNotification;
			}
		}

		btnUpdateNotifications.Clicked += OnBtnUpdateNotificationClicked;

		#endregion

		#region Уведомление о наличии незакрытых рекламаций без комментариев в добавленной дискуссии для отдела

		_complaintNotificationController = autofacScope.Resolve<IComplaintNotificationController>(new TypedParameter(typeof(int), _currentUserSubdivisionId));

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
			userIsSalesRepresentative = commonServices.CurrentPermissionService.ValidatePresetPermission("user_is_sales_representative")
				&& !commonServices.UserService.GetCurrentUser(uow).IsAdmin;
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
	}

	private void ActionProfitabilitySalesReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(ProfitabilitySalesReportViewModel));
	}

	#region Уведомления об отправленных перемещениях и о наличии рекламаций
	private int GetEmployeeSubdivisionId(IUnitOfWork uow)
	{
		var currentEmployee =
			VodovozGtkServicesConfig.EmployeeService.GetEmployeeForUser(uow, ServicesConfig.UserService.CurrentUserId);

		return currentEmployee?.Subdivision.Id ?? 0;
	}

	#region Методы для уведомления об отправленных перемещениях для подразделения
	private void OnBtnUpdateNotificationClicked(object sender, EventArgs e)
	{
		using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			var movementsNotification = _movementsNotificationsController.GetNotificationMessageBySubdivision(uow);
			UpdateSendedMovementsNotification(movementsNotification);
		}

		if(!_hideComplaintsNotifications)
		{
			var complaintsNotifications = GetComplaintNotificationDetails();
			UpdateSendedComplaintsNotification(complaintsNotifications);
		}
	}

	private void UpdateSendedMovementsNotification(string notification)
	{
		lblMovementsNotification.Markup = notification;
	}
	#endregion

	#region Методы для уведомления о наличии незакрытых рекламаций без комментариев для подразделения
	private void UpdateSendedComplaintsNotification(SendedComplaintNotificationDetails notificationDetails)
	{
		lblComplaintsNotification.Markup = notificationDetails.NotificationMessage;
		hboxComplaintsNotification.Visible = notificationDetails.NeedNotify;
	}

	private SendedComplaintNotificationDetails GetComplaintNotificationDetails()
	{
		SendedComplaintNotificationDetails notificationDetails;

		using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			notificationDetails = _complaintNotificationController.GetNotificationDetails(uow);
		}

		return notificationDetails;
	}

	private void OnBtnOpenComplaintClicked(object sender, EventArgs e)
	{
		var notificationDetails = GetComplaintNotificationDetails();

		UpdateSendedComplaintsNotification(notificationDetails);

		if(notificationDetails.SendedComplaintsCount > 0)
		{
			NavigationManager.OpenViewModel<ComplaintViewModel, IEntityUoWBuilder>(
				null,
				EntityUoWBuilder.ForOpen(notificationDetails.SendedComplaintsIds.Min()),
				OpenPageOptions.None
				);
		}
	}
	#endregion

	#endregion

	protected void OnAboutActionActivated(object sender, EventArgs e)
	{
		var aboutViewModel = new AboutViewModel(applicationInfo);
		var aboutView = new AboutView(aboutViewModel);
		aboutView.ShowAll();
		aboutView.Run();
		aboutView.Destroy();
	}

	protected void OnActionParametersActivated(object sender, EventArgs e)
	{
		var baseParametersViewModel = new BaseParametersViewModel(
			NavigationManager,
			new ParametersService(QS.Project.DB.Connection.ConnectionDB));
		var baseParametersView = new BaseParametersView(baseParametersViewModel);
		baseParametersView.ShowAll();
		baseParametersView.Run();
		baseParametersView.Destroy();
	}

	protected void OnActionDocTemplatesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DocTemplate>(),
			() => new OrmReference(typeof(DocTemplate))
		);
	}

	protected void OnActionEmployeeFinesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.EmployeesFines>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.EmployeesFines())
		);
	}

	protected void OnActionStockMovementsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.StockMovements>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.StockMovements())
		);
	}

	protected void OnActionComplaintsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ComplaintsJournalsViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnActionSalesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.SalesReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.SalesReport(new EmployeeRepository(), ServicesConfig.InteractiveService))
		);
	}

	protected void OnActionSalesByDicountReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SalesByDiscountReport>(),
			() => new QSReport.ReportViewDlg(new SalesByDiscountReport())
		);
	}

	protected void OnActionDriverWagesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.DriverWagesReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.DriverWagesReport())
		);
	}

	protected void OnActionFuelReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.FuelReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.FuelReport())
		);
	}

	protected void OnActionShortfallBattlesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Bottles.ShortfallBattlesReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Bottles.ShortfallBattlesReport())
		);
	}

	protected void OnActionWagesOperationsActivated(object sender, EventArgs e)
	{
		EmployeeFilterViewModel employeeFilter;
		if(_hasAccessToSalariesForLogistics)
		{
			employeeFilter = new EmployeeFilterViewModel(EmployeeCategory.office);
			employeeFilter.SetAndRefilterAtOnce(
				x => x.Category = EmployeeCategory.driver,
				x => x.Status = EmployeeStatus.IsWorking);
		}
		else
		{
			employeeFilter = new EmployeeFilterViewModel();
			employeeFilter.SetAndRefilterAtOnce(x => x.Status = EmployeeStatus.IsWorking);
		}

		employeeFilter.HidenByDefault = true;
		var employeeJournalFactory = new EmployeeJournalFactory(employeeFilter);

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.WagesOperationsReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.WagesOperationsReport(employeeJournalFactory))
		);
	}

	protected void OnActionEquipmentReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.EquipmentReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.EquipmentReport(ServicesConfig.InteractiveService))
		);
	}

	protected void OnActionForwarderWageReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.ForwarderWageReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.ForwarderWageReport())
		);
	}

	protected void OnActionCashierCommentsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.CashierCommentsReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.CashierCommentsReport())
		);
	}

	protected void OnActionCommentsForLogistsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OnecCommentsReport>(),
			() => new QSReport.ReportViewDlg(new OnecCommentsReport())
		);
	}

	protected void OnActionDriversWageBalanceActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DriversWageBalanceReport>(),
			() => new QSReport.ReportViewDlg(new DriversWageBalanceReport())
		);
	}



	protected void OnActionDeliveriesLateActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.Logistic.DeliveriesLateReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.Logistic.DeliveriesLateReport())
		);
	}

	protected void OnActionQualityRetailReport(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<QualityReport>(),
			() => new QSReport.ReportViewDlg(new QualityReport(
				new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope()),
				new EmployeeJournalFactory(),
				new SalesChannelJournalFactory(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.InteractiveService)));
	}

	protected void OnActionRoutesListRegisterActivated(object sender, EventArgs e) => OpenDriverRoutesListRegisterReport();
	protected void OnActionOrderedByIdRoutesListRegisterActivated(object sender, EventArgs e) => OpenRoutesListRegisterReport();
	protected void OnActionProducedProductionReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ProducedProductionReport>(),
			() => new QSReport.ReportViewDlg(
				new ProducedProductionReport(new NomenclatureJournalFactory()))
		);
	}

	protected void OpenRoutesListRegisterReport()
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.Logistic.RoutesListRegisterReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.Logistic.RoutesListRegisterReport())
		);
	}

	protected void OpenDriverRoutesListRegisterReport()
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DriverRoutesListRegisterReport>(),
			() => new QSReport.ReportViewDlg(new DriverRoutesListRegisterReport())
		);
	}

	protected void OnActionDeliveryTimeReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(QSReport.ReportViewDlg.GenerateHashName<DeliveryTimeReport>(),
			() => new QSReport.ReportViewDlg(
				new DeliveryTimeReport(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.InteractiveService))
		);
	}

	protected void OnActionOrdersByDistrict(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersByDistrictReport>(),
			() => new QSReport.ReportViewDlg(new OrdersByDistrictReport())
		);
	}

	protected void OnActionCompanyTrucksActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CompanyTrucksReport>(),
			() => new QSReport.ReportViewDlg(new CompanyTrucksReport())
		);
	}

	protected void OnActionLastOrderReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<LastOrderByDeliveryPointReport>(),
			() => new QSReport.ReportViewDlg(new LastOrderByDeliveryPointReport())
		);
	}

	protected void OnActionOrderIncorrectPricesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderIncorrectPrices>(),
			() => new QSReport.ReportViewDlg(new OrderIncorrectPrices())
		);
	}

	protected void OnActionAddressDuplicetesActivated(object sender, EventArgs e)
	{
		IParametersProvider parametersProvider = new ParametersProvider();
		IFiasApiParametersProvider fiasApiParametersProvider = new FiasApiParametersProvider(parametersProvider);
		var geoCoderCache = new GeocoderCache(UnitOfWorkFactory.GetDefaultFactory);
		IFiasApiClient fiasApiClient = new FiasApiClient(fiasApiParametersProvider.FiasApiBaseUrl, fiasApiParametersProvider.FiasApiToken, geoCoderCache);

		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<MergeAddressesDlg>(),
			() => new MergeAddressesDlg(fiasApiClient)
		);
	}

	protected void OnActionOrdersWithMinPriceLessThanActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersWithMinPriceLessThan>(),
			() => new QSReport.ReportViewDlg(new OrdersWithMinPriceLessThan())
		);
	}

	protected void OnActionRouteListsOnClosingActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.RouteListsOnClosingReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.RouteListsOnClosingReport())
		);
	}

	protected void OnActionOnLoadTimeActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.OnLoadTimeAtDayReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.OnLoadTimeAtDayReport())
		);
	}

	protected void OnActionSelfDeliveryReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SelfDeliveryReport>(),
			() => new QSReport.ReportViewDlg(new SelfDeliveryReport())
		);
	}


	protected void OnActionShipmentReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.ShipmentReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.ShipmentReport())
		);
	}

	protected void OnActionBottlesMovementReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<BottlesMovementReport>(),
			() => new QSReport.ReportViewDlg(new BottlesMovementReport())
		);
	}

	protected void OnActionMileageReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<MileageReport>(),
			() => new QSReport.ReportViewDlg(
				new MileageReport(
					autofacScope.Resolve<IEmployeeJournalFactory>(),
					autofacScope.Resolve<ICarJournalFactory>()
				)
			)
		);
	}

	protected void OnActionMastersReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<MastersReport>(),
			() => new QSReport.ReportViewDlg(new MastersReport())
		);
	}

	protected void OnActionSuburbWaterPriceActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Sales.SuburbWaterPriceReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Sales.SuburbWaterPriceReport())
		);
	}

	protected void OnActionDistanceFromCenterActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<CalculateDistanceToPointsDlg>(),
			() => new CalculateDistanceToPointsDlg()
		);
	}

	protected void OnActionOrdersWithoutBottlesOperationActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<OrdersWithoutBottlesOperationDlg>(),
			() => new OrdersWithoutBottlesOperationDlg()
		);
	}


	protected void OnAction45Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ReplaceEntityLinksDlg>(),
			() => new ReplaceEntityLinksDlg()
		);
	}

	protected void OnActionBottlesMovementSummaryReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Bottles.BottlesMovementSummaryReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Bottles.BottlesMovementSummaryReport())
		);
	}

	protected void OnActionDriveingCallsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.DrivingCallReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.DrivingCallReport())
		);
	}

	protected void OnActionMastersVisitReportActivated(object sender, EventArgs e)
	{
		var employeeFactory = new EmployeeJournalFactory();
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<MastersVisitReport>(),
			() => new QSReport.ReportViewDlg(new MastersVisitReport(employeeFactory))
		);
	}

	protected void OnActionNotDeliveredOrdersActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<NotDeliveredOrdersReport>(),
			() => new QSReport.ReportViewDlg(new NotDeliveredOrdersReport())
		);
	}

	protected void OnActionCounterpartyTagsActivated(object sender, EventArgs e)
	{
		var refWin = new OrmReference(typeof(Tag));
		tdiMain.AddTab(refWin);
	}


	protected void OnAction48Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EmployeesPremiums>(),
			() => new QSReport.ReportViewDlg(new EmployeesPremiums())
		);
	}

	protected void OnActionOrderStatisticByWeekReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderStatisticByWeekReport>(),
			() => new QSReport.ReportViewDlg(new OrderStatisticByWeekReport())
		);
	}

	protected void OnReportKungolovoActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ReportForBigClient>(),
			() => new QSReport.ReportViewDlg(new ReportForBigClient())
		);
	}

	protected void OnActionLoad1cCounterpartyAndDeliveryPointsActivated(object sender, EventArgs e)
	{
		var widget = new LoadFrom1cClientsAndDeliveryPoints();
		tdiMain.AddTab(widget);
	}

	protected void OnActionOrderRegistryActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderRegistryReport>(),
			() => new QSReport.ReportViewDlg(new OrderRegistryReport())
		);
	}

	protected void OnActionEquipmentBalanceActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Store.EquipmentBalance>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Store.EquipmentBalance())
		);
	}

	protected void OnActionCardPaymentsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CardPaymentsOrdersReport>(),
			() => new QSReport.ReportViewDlg(new CardPaymentsOrdersReport(UnitOfWorkFactory.GetDefaultFactory))
		);
	}

	protected void OnActionToOnlineStoreActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ExportToSiteDlg>(),
			() => new ExportToSiteDlg()
		);
	}

	protected void OnActionDefectiveItemsReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DefectiveItemsReport>(),
			() => new QSReport.ReportViewDlg(new DefectiveItemsReport())
		);
	}

	protected void OnActionTraineeActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			PermissionControlledRepresentationJournal.GenerateHashName<TraineeVM>(),
			() => new PermissionControlledRepresentationJournal(new TraineeVM())
		);
	}

	protected void OnOnLineActionActivated(object sender, EventArgs e)
	{
		var paymentsRepository = new PaymentsRepository();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PaymentsFromTinkoffReport>(),
			() => new QSReport.ReportViewDlg(new PaymentsFromTinkoffReport(paymentsRepository))
		);
	}

	protected void OnActionOrdersByDistrictsAndDeliverySchedulesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersByDistrictsAndDeliverySchedulesReport>(),
			() => new QSReport.ReportViewDlg(new OrdersByDistrictsAndDeliverySchedulesReport())
		);
	}

	protected void OnActionOrdersByCreationDate(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersByCreationDateReport>(),
			() => new QSReport.ReportViewDlg(new OrdersByCreationDateReport())
		);
	}

	protected void OnActionTypesOfEntitiesActivated(object sender, EventArgs e)
	{
		if(QSMain.User.Admin)
			tdiMain.OpenTab(
				OrmReference.GenerateHashName<TypeOfEntity>(),
				() => new OrmReference(typeof(TypeOfEntity))
			);
	}

	protected void OnActionUsersActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UsersJournalViewModel>(null);
	}

	protected void OnActionGeographicGroupsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<GeoGroupJournalViewModel>(null);
	}

	protected void OnForShipmentReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<NomenclatureForShipment>(),
			() => new QSReport.ReportViewDlg(new NomenclatureForShipment())
		);
	}

	protected void OnImageListOpenActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<StoredResource>(),
			() => new OrmReference(typeof(StoredResource))
		);
	}

	protected void OnActionOrderCreationDateReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Sales.OrderCreationDateReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Sales.OrderCreationDateReport())
		);
	}

	protected void OnActionNotFullyLoadedRouteListsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<NotFullyLoadedRouteListsReport>(),
			() => new QSReport.ReportViewDlg(new NotFullyLoadedRouteListsReport())
		);
	}

	protected void OnActionFirstClientsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<FirstClientsReport>(),
			() => new QSReport.ReportViewDlg(
				  new FirstClientsReport(
						new DistrictJournalFactory(),
						new DiscountReasonRepository())));
	}

	protected void OnActionTariffZoneDebtsReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<TariffZoneDebts>(),
			() => new QSReport.ReportViewDlg(new TariffZoneDebts())
		);
	}

	protected void OnActionStockMovementsAdvancedReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<StockMovementsAdvancedReport>(),
			() => new QSReport.ReportViewDlg(new StockMovementsAdvancedReport())
		);
	}

	protected void OnActionCounterpartyActivityKindActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ClientsByDeliveryPointCategoryAndActivityKindsReport>(),
			() => new QSReport.ReportViewDlg(new ClientsByDeliveryPointCategoryAndActivityKindsReport())
		);
	}

	protected void OnActionExtraBottlesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ExtraBottleReport>(),
			() => new QSReport.ReportViewDlg(new ExtraBottleReport())
		);
	}

	protected void OnActionFirstSecondReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<FirstSecondClientReport>(),
			() => new QSReport.ReportViewDlg(new FirstSecondClientReport(new DiscountReasonRepository()))
		);
	}

	protected void OnActionFuelConsumptionReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<FuelConsumptionReport>(),
			() => new QSReport.ReportViewDlg(new FuelConsumptionReport())
		);
	}

	protected void OnActionCloseDeliveryReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CounterpartyCloseDeliveryReport>(),
			() => new QSReport.ReportViewDlg(new CounterpartyCloseDeliveryReport())
		);
	}

	protected void OnIncomeBalanceReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<IncomeBalanceReport>(),
			() => new QSReport.ReportViewDlg(new IncomeBalanceReport())
		);
	}

	protected void OnCashBoolReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CashBookReport>(),
			() => new QSReport.ReportViewDlg(new CashBookReport(
				new SubdivisionRepository(new ParametersProvider()), ServicesConfig.CommonServices))
		);
	}

	protected void OnActionProfitabilityBottlesByStockActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ProfitabilityBottlesByStockReport>(),
			() => new QSReport.ReportViewDlg(new ProfitabilityBottlesByStockReport())
		);
	}

	protected void OnAction62Activated(object sender, EventArgs e)
	{
		var widget = new ResendEmailsDialog();
		tdiMain.AddTab(widget);
	}

	protected void OnActionPlanImplementationReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PlanImplementationReport>(),
			() => new QSReport.ReportViewDlg(new PlanImplementationReport())
		);
	}

	protected void OnActionZeroDebtClientReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ZeroDebtClientReport>(),
			() => new QSReport.ReportViewDlg(new ZeroDebtClientReport())
		);
	}

	protected void OnActionSetBillsReportActivated(object sender, EventArgs e)
	{
		var subdivisionJournalFactory = new SubdivisionJournalFactory();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SetBillsReport>(),
			() => new QSReport.ReportViewDlg(new SetBillsReport(
				UnitOfWorkFactory.GetDefaultFactory,
				subdivisionJournalFactory))
		);
	}

	protected void OnActionOrdersCreationTimeReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrdersCreationTimeReport>(),
			() => new QSReport.ReportViewDlg(new OrdersCreationTimeReport())
		);
	}

	protected void OnAction66Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PotentialFreePromosetsReport>(),
			() => new QSReport.ReportViewDlg(new PotentialFreePromosetsReport())
		);
	}

	protected void OnActionWayBillReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<WayBillReport>(),
			() => new QSReport.ReportViewDlg(
				new WayBillReportGroupPrint(
					autofacScope.Resolve<IEmployeeJournalFactory>(),
					autofacScope.Resolve<ICarJournalFactory>(),
					autofacScope.Resolve<IOrganizationJournalFactory>(),
					autofacScope.Resolve<IInteractiveService>(),
					autofacScope.Resolve<ISubdivisionRepository>()
				)
			)
		);
	}

	protected void OnActionPaymentsReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PaymentsFromBankClientReport>(),
			() => new QSReport.ReportViewDlg(
				new PaymentsFromBankClientReport(new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope()), new UserRepository(), ServicesConfig.CommonServices))
		);
	}

	protected void OnActionPaymentsFinDepartmentReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PaymentsFromBankClientFinDepartmentReport>(),
			() => new QSReport.ReportViewDlg(new PaymentsFromBankClientFinDepartmentReport())
		);
	}
	protected void OnActionNetworkDelayReportActivated(object sender, EventArgs e)
	{
		ILifetimeScope lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		var employeeJournalFactory = new EmployeeJournalFactory();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ChainStoreDelayReport>(),
			() => new QSReport.ReportViewDlg(new ChainStoreDelayReport(employeeJournalFactory, lifetimeScope.Resolve<ICounterpartyJournalFactory>()))
		);
	}

	protected void OnActionReturnedTareReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ReturnedTareReport>(),
			() => new QSReport.ReportViewDlg(new ReturnedTareReport(ServicesConfig.InteractiveService))
		);
	}

	protected void OnActionProductionRequestReportActivated(object sender, EventArgs e)
	{
		var employeeRepository = new EmployeeRepository();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ProductionRequestReport>(),
			() => new QSReport.ReportViewDlg(new ProductionRequestReport(employeeRepository))
		);
	}

	protected void OnActionScheduleOnLinePerShiftReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<FuelConsumptionReport>(),
			() => new QSReport.ReportViewDlg(new ScheduleOnLinePerShiftReport())
		);
	}

	protected void OnActionNonClosedRLByPeriodReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<NonClosedRLByPeriodReport>(),
			() => new QSReport.ReportViewDlg(new NonClosedRLByPeriodReport())
		);
	}

	protected void OnActionCashRequestReportActivated(object sender, EventArgs e)
	{
		var employeeFilter = new EmployeeFilterViewModel
		{
			Status = EmployeeStatus.IsWorking,
		};

		var employeeJournalFactory = new EmployeeJournalFactory(employeeFilter);

		var page = NavigationManager.OpenViewModel<PayoutRequestsJournalViewModel, IEmployeeJournalFactory, bool, bool>
			(null, employeeJournalFactory, false, false, OpenPageOptions.IgnoreHash);
		page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
	}

	protected void OnActionOpenProposalsJournalActivated(object sender, EventArgs e)
	{
		var filter = new ApplicationDevelopmentProposalsJournalFilterViewModel { HidenByDefault = true };

		tdiMain.AddTab(
			new ApplicationDevelopmentProposalsJournalViewModel(
				filter,
				new EmployeeService(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			)
			{ SelectionMode = JournalSelectionMode.Multiple }
		);
	}

	protected void OnAction71Activated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EShopSalesReport>(),
			() => new QSReport.ReportViewDlg(new EShopSalesReport())
		);
	}

	protected void OnActionWayBillJournalActivated(object sender, EventArgs e)
	{
		var employeeFilter = new EmployeeFilterViewModel
		{
			Status = EmployeeStatus.IsWorking
		};

		var employeesJournalFactory = new EmployeeJournalFactory(employeeFilter);
		var docTemplateRepository = new DocTemplateRepository();
		var fileChooser = new Vodovoz.FileChooser();

		tdiMain.OpenTab(
			() => new WayBillGeneratorViewModel
			(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices.InteractiveService,
				NavigationManagerProvider.NavigationManager,
				new WayBillDocumentRepository(),
				new RouteGeometryCalculator(),
				employeesJournalFactory,
				docTemplateRepository,
				fileChooser
			));
	}

	protected void OnActionOrderChangesReportActivated(object sender, EventArgs e)
	{
		var paramProvider = new ParametersProvider();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderChangesReport>(),
			() => new QSReport.ReportViewDlg(
				new OrderChangesReport(
					new ReportDefaultsProvider(paramProvider),
					ServicesConfig.InteractiveService,
					new ArchiveDataSettings(paramProvider)))
		);
	}

	protected void OnRegisteredRMActionActivated(object sender, EventArgs e)
	{
		tdiMain.AddTab(
			new RegisteredRMJournalViewModel(
				new RegisteredRMJournalFilterViewModel(),
				UnitOfWorkFactory.GetDefaultFactory,
				new PermissionRepository(),
				ServicesConfig.CommonServices
			)
		);
	}

	protected void OnActionRetailComplaintsJournalActivated(object sender, EventArgs e)
	{
		Action<ComplaintFilterViewModel> action = (filterConfig) => filterConfig.IsForRetail = true;

		var filter = autofacScope.BeginLifetimeScope().Resolve<ComplaintFilterViewModel>(new TypedParameter(typeof(Action<ComplaintFilterViewModel>), action));

		NavigationManager.OpenViewModel<ComplaintsJournalViewModel, ComplaintFilterViewModel>(
			   null,
			   filter,
			   OpenPageOptions.IgnoreHash);
	}

	protected void OnActionRetailUndeliveredOrdersJournalActivated(object sender, EventArgs e)
	{
		MessageDialogHelper.RunInfoDialog("Журнал недовозов");
	}

	protected void OnActionRetailCounterpartyJournalActivated(object sender, EventArgs e)
	{
		CounterpartyJournalFilterViewModel filter = new CounterpartyJournalFilterViewModel() { IsForRetail = true };
		var counterpartyJournal = new RetailCounterpartyJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);

		tdiMain.OpenTab(
			() => counterpartyJournal
		);
	}

	protected void OnActionRetailOrdersJournalActivated(object sender, EventArgs e)
	{
		var counterpartyJournalFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());
		var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
		var employeeJournalFactory = new EmployeeJournalFactory();

		var orderJournalFilter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, employeeJournalFactory)
		{
			IsForRetail = true
		};
		NavigationManager.OpenViewModel<RetailOrderJournalViewModel, OrderJournalFilterViewModel>(null, orderJournalFilter);
	}

	protected void OnActionCarsExploitationReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CarsExploitationReportViewModel>(null);
	}

	protected void OnActionRecalculateDriverWagesActivated(object sender, EventArgs e)
	{
		var dlg = new RecalculateDriverWageDlg();
		tdiMain.AddTab(dlg);
	}

	protected void OnActionDriversInfoExportActivated(object sender, EventArgs e)
	{
		var wageParameterService =
			new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(new ParametersProvider()));

		tdiMain.AddTab(
			new DriversInfoExportViewModel(
				wageParameterService,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.InteractiveService,
				null)
		);
	}

	protected void OnActionCounterpartyRetailReport(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CounterpartyReport>(),
			() => new QSReport.ReportViewDlg(new CounterpartyReport(
				new SalesChannelJournalFactory(),
				new DistrictJournalFactory(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.InteractiveService)));
	}

	protected void OnDriversToDistrictsAssignmentReportActionActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<DriversToDistrictsAssignmentReport>(),
			() => new QSReport.ReportViewDlg(new DriversToDistrictsAssignmentReport())
		);
	}

	protected void OnActionNomenclaturePlanReportActivated(object sender, EventArgs e)
	{
		IProductGroupJournalFactory productGroupJournalFactory = new ProductGroupJournalFactory();
		IParametersProvider parametersProvider = new ParametersProvider();
		INomenclaturePlanParametersProvider nomenclaturePlanParametersProvider = new NomenclaturePlanParametersProvider(parametersProvider);
		IFileDialogService fileDialogService = new FileDialogService();

		NomenclaturePlanReportViewModel viewModel = new NomenclaturePlanReportViewModel(UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.InteractiveService, NavigationManager, ServicesConfig.CommonServices, productGroupJournalFactory, nomenclaturePlanParametersProvider,
			fileDialogService);

		tdiMain.AddTab(viewModel);
	}

	protected void OnLogisticsGeneralSalaryInfoActivated(object sender, EventArgs e)
	{
		var filter = new EmployeeFilterViewModel
		{
			Category = EmployeeCategory.driver
		};

		var employeeJournalFactory = new EmployeeJournalFactory(filter);

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<GeneralSalaryInfoReport>(),
			() => new QSReport.ReportViewDlg(new GeneralSalaryInfoReport(employeeJournalFactory, ServicesConfig.InteractiveService))
		);
	}

	protected void OnActionOrderAnalyticsReportActivated(object sender, EventArgs e)
	{
		var uowFactory = autofacScope.Resolve<IUnitOfWorkFactory>();
		var interactiveService = autofacScope.Resolve<IInteractiveService>();

		NavigationManager.OpenViewModel<OrderAnalyticsReportViewModel, INavigationManager, IUnitOfWorkFactory, IInteractiveService>(
			null, NavigationManager, uowFactory, interactiveService);
	}

	protected void OnActionEmployeesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EmployeesReport>(),
			() => new QSReport.ReportViewDlg(new EmployeesReport(ServicesConfig.InteractiveService))
		);
	}

	protected void OnActionAddressesOverpaymentsReportActivated(object sender, EventArgs e)
	{
		var driverFilter = new EmployeeFilterViewModel { RestrictCategory = EmployeeCategory.driver };
		var employeeJournalFactory = new EmployeeJournalFactory(driverFilter);

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<AddressesOverpaymentsReport>(),
			() => new QSReport.ReportViewDlg(new AddressesOverpaymentsReport(
				employeeJournalFactory,
				ServicesConfig.InteractiveService))
		);
	}

	protected void OnActionDeliveryAnalyticsActivated(object sender, EventArgs e)
	{
		var districtJournalFactory = new DistrictJournalFactory();

		tdiMain.AddTab(
			new DeliveryAnalyticsViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.InteractiveService,
				NavigationManager,
				districtJournalFactory)
		);
	}

	protected void OnActionDayOfSalaryGiveoutReport_Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RdlViewerViewModel, Type>(null, typeof(DayOfSalaryGiveoutReportViewModel));
	}

	protected void OnProductionWarehouseMovementReportActivated(object sender, EventArgs e)
	{
		IFileDialogService fileDialogService = new FileDialogService();
		IParametersProvider parametersProvider = new ParametersProvider();
		IProductionWarehouseMovementReportProvider productionWarehouseMovementReportProvider = new ProductionWarehouseMovementReportProvider(parametersProvider);

		ProductionWarehouseMovementReportViewModel viewModel = new ProductionWarehouseMovementReportViewModel(UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.InteractiveService, NavigationManager, fileDialogService, productionWarehouseMovementReportProvider);

		tdiMain.AddTab(viewModel);
	}

	protected void OnActionSalaryRatesReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SalaryRatesReport>(),
			() => new QSReport.ReportViewDlg(new SalaryRatesReport(
				UnitOfWorkFactory.GetDefaultFactory,
				new BaseParametersProvider(new ParametersProvider()),
				ServicesConfig.CommonServices
			)));
	}

	protected void OnActionAnalyticsForUndeliveryActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<AnalyticsForUndeliveryReport>(),
			() => new QSReport.ReportViewDlg(new AnalyticsForUndeliveryReport())
		);
	}

	protected void OnActionCounterpartyCashlessDebtsReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CounterpartyCashlessDebtsReport>(),
			() => new QSReport.ReportViewDlg(autofacScope.Resolve<CounterpartyCashlessDebtsReport>())
		);
	}

	protected void OnActionPaymentsFromAvangardReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PaymentsFromAvangardReport>(),
			() => new QSReport.ReportViewDlg(new PaymentsFromAvangardReport())
		);
	}

	protected void OnActionCostCarExploitationReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CostCarExploitationReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnFastDeliverySalesReportActionActivated(object sender, EventArgs e)
	{
		IFileDialogService fileDialogService = new FileDialogService();

		FastDeliverySalesReportViewModel viewModel = new FastDeliverySalesReportViewModel(UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.InteractiveService, NavigationManager, fileDialogService);

		tdiMain.AddTab(viewModel);
	}

	protected void OnFastDeliveryAdditionalLoadingReportActionActivated(object sender, EventArgs e)
	{
		IFileDialogService fileDialogService = new FileDialogService();

		FastDeliveryAdditionalLoadingReportViewModel viewModel = new FastDeliveryAdditionalLoadingReportViewModel(UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.InteractiveService, NavigationManager, fileDialogService);

		tdiMain.AddTab(viewModel);
	}
	
	protected void OnActionBulkEmailEventsReportActivated(object sender, EventArgs e)
	{
		ICounterpartyJournalFactory counterpartyJournalFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());
		IBulkEmailEventReasonJournalFactory bulkEmailEventReasonJournalFactory = new BulkEmailEventReasonJournalFactory();
		IFileDialogService fileDialogService = new FileDialogService();

		BulkEmailEventReportViewModel viewModel = new BulkEmailEventReportViewModel(UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.InteractiveService, NavigationManager, fileDialogService, bulkEmailEventReasonJournalFactory, counterpartyJournalFactory);

		tdiMain.AddTab(viewModel);
	}

	protected void OnActionEdoUpdReportActivated(object sender, EventArgs e)
	{
		IFileDialogService fileDialogService = new FileDialogService();

		var edoUpdReportViewModel = new EdoUpdReportViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.InteractiveService, NavigationManager,
			fileDialogService);

		tdiMain.AddTab(edoUpdReportViewModel);
	}

	protected void OnUsersRolesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UserRolesJournalViewModel>(null);
	}

	protected void OnEmployeesTaxesActionActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<EmployeesTaxesSumReport>(),
			() => new QSReport.ReportViewDlg(new EmployeesTaxesSumReport(UnitOfWorkFactory.GetDefaultFactory))
		);
	}

	protected void OnActionTurnoverWithDynamicsReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<TurnoverWithDynamicsReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnActionFastDeliveryPercentCoverageReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FastDeliveryPercentCoverageReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnSalesBySubdivisionsAnalitycsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<SalesBySubdivisionsAnalitycsReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	private DateTime GetDateTimeFGromVersion(Version version) =>
		new DateTime(2000, 1, 1)
			.AddDays(version.Build)
			.AddSeconds(version.Revision * 2);

	protected void OnInventoryInstanceMovementReportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<InventoryInstanceMovementReportViewModel>(null);
	}

	protected void OnActionActionWarehousesBalanceSummaryReportActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<WarehousesBalanceSummaryViewModel>(null);
	}
}
