using Autofac;
using Fias.Client;
using Fias.Client.Cache;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QS.Banks.Domain;
using QS.BaseParameters;
using QS.BaseParameters.ViewModels;
using QS.BaseParameters.Views;
using QS.BusinessCommon.Domain;
using QS.ChangePassword.Views;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Repositories;
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
using QS.ViewModels;
using QSBanks;
using QSOrmProject;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Vodovoz;
using Vodovoz.Controllers;
using Vodovoz.Core;
using Vodovoz.Core.DataService;
using Vodovoz.Core.Journal;
using Vodovoz.Dialogs.OnlineStore;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.StoredResources;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using Vodovoz.JournalViewers;
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
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Accounting;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs.Complaints;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Dialogs.Goods;
using Vodovoz.ViewModels.Dialogs.Roboats;
using Vodovoz.ViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;
using Vodovoz.ViewModels.Journals.FilterViewModels.Retail;
using Vodovoz.ViewModels.Journals.FilterViewModels.Security;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints.ComplaintResults;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Flyers;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.Journals.JournalViewModels.Proposal;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;
using Vodovoz.ViewModels.Journals.JournalViewModels.Retail;
using Vodovoz.ViewModels.Journals.JournalViewModels.Sale;
using Vodovoz.ViewModels.Journals.JournalViewModels.Security;
using Vodovoz.ViewModels.Journals.JournalViewModels.Users;
using Vodovoz.ViewModels.Profitability;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ReportsParameters.Cash;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.Users;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport;
using Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using Vodovoz.ViewModels.ViewModels.Reports.Sales;
using Vodovoz.ViewModels.ViewModels.Settings;
using VodovozInfrastructure.Configuration;
using VodovozInfrastructure.Passwords;
using Connection = QS.Project.DB.Connection;
using Order = Vodovoz.Domain.Orders.Order;
using ToolbarStyle = Vodovoz.Domain.Employees.ToolbarStyle;
using UserRepository = Vodovoz.EntityRepositories.UserRepository;
using Vodovoz.ViewModels.ViewModels.Warehouses;

public partial class MainWindow : Gtk.Window
{
	private static Logger logger = LogManager.GetCurrentClassLogger();
	private uint lastUiId;
	private readonly ILifetimeScope autofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
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

	public void OnTdiMainTabAdded(object sender, TabAddedEventArgs args)
	{
		switch(args.Tab)
		{
			case IInfoProvider dialogTab:
				dialogTab.CurrentObjectChanged += infopanel.OnCurrentObjectChanged;
				break;
			case TdiSliderTab journalTab when journalTab.Journal is IInfoProvider journal:
				journal.CurrentObjectChanged += infopanel.OnCurrentObjectChanged;
				break;
			case TdiSliderTab tdiSliderTab
				when(tdiSliderTab.Journal as MultipleEntityJournal)?.RepresentationModel is IInfoProvider provider:
				{
					provider.CurrentObjectChanged += infopanel.OnCurrentObjectChanged;
					break;
				}
		}
	}

	public void OnTdiMainTabClosed(object sender, TabClosedEventArgs args)
	{
		switch(args.Tab)
		{
			case IInfoProvider dialogTab:
				infopanel.OnInfoProviderDisposed(dialogTab);
				break;
			case TdiSliderTab journalTab when journalTab.Journal is IInfoProvider journal:
				infopanel.OnInfoProviderDisposed(journal);
				break;
			case TdiSliderTab tdiSliderTab
				when(tdiSliderTab.Journal as MultipleEntityJournal)?.RepresentationModel is IInfoProvider provider:
				{
					infopanel.OnInfoProviderDisposed(provider);
					break;
				}
		}

		if(tdiMain.NPages == 0)
		{
			infopanel.SetInfoProvider(DefaultInfoProvider.Instance);
		}
	}

	public void OnTdiMainTabSwitched(object sender, TabSwitchedEventArgs args)
	{
		var currentTab = args.Tab;
		switch(currentTab)
		{
			case IInfoProvider provider:
				infopanel.SetInfoProvider(provider);
				break;
			case TdiSliderTab tdiSliderTab when tdiSliderTab.Journal is IInfoProvider provider:
				infopanel.SetInfoProvider(provider);
				break;
			case TdiSliderTab tdiSliderTab when
				(tdiSliderTab.Journal as MultipleEntityJournal)?.RepresentationModel is IInfoProvider provider:
				{
					infopanel.SetInfoProvider(provider);
					break;
				}
			default:
				infopanel.SetInfoProvider(DefaultInfoProvider.Instance);
				break;
		}
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		if(tdiMain.CloseAllTabs())
		{
			a.RetVal = false;
			autofacScope.Dispose();
			Application.Quit();
		}
		else
		{
			a.RetVal = true;
		}
	}

	protected void OnQuitActionActivated(object sender, EventArgs e)
	{
		if(tdiMain.CloseAllTabs())
		{
			autofacScope.Dispose();
			Application.Quit();
		}
	}

	protected void OnDialogAuthenticationActionActivated(object sender, EventArgs e)
	{
		if(!(Connection.ConnectionDB is MySqlConnection mySqlConnection))
		{
			throw new InvalidOperationException($"Текущее подключение не является {nameof(MySqlConnection)}");
		}
		var mySqlPasswordRepository = new MySqlPasswordRepository();
		var changePasswordModel = new MysqlChangePasswordModelExtended(applicationConfigurator, mySqlConnection, mySqlPasswordRepository);
		var changePasswordViewModel = new ChangePasswordViewModel(changePasswordModel, passwordValidator, null);
		var changePasswordView = new ChangePasswordView(changePasswordViewModel);

		changePasswordView.ShowAll();
		changePasswordView.Run();
		changePasswordView.Destroy();
	}

	protected void OnAboutActionActivated(object sender, EventArgs e)
	{
		var aboutViewModel = new AboutViewModel(applicationInfo);
		var aboutView = new AboutView(aboutViewModel);
		aboutView.ShowAll();
		aboutView.Run();
		aboutView.Destroy();
	}

	protected void OnActionOrdersToggled(object sender, EventArgs e)
	{
		if(ActionOrders.Active)
			SwitchToUI("Vodovoz.toolbars.orders.xml");
	}

	private void SwitchToUI(string uiResource)
	{
		if(lastUiId > 0)
		{
			this.UIManager.RemoveUi(lastUiId);
			lastUiId = 0;
		}
		lastUiId = this.UIManager.AddUiFromResource(uiResource);
		this.UIManager.EnsureUpdate();
	}

	protected void OnActionServicesToggled(object sender, EventArgs e)
	{
		if(ActionServices.Active)
			SwitchToUI("Vodovoz.toolbars.services.xml");
	}

	protected void OnActionLogisticsToggled(object sender, EventArgs e)
	{
		if(ActionLogistics.Active)
			SwitchToUI("logistics.xml");
	}

	protected void OnActionStockToggled(object sender, EventArgs e)
	{
		if(ActionStock.Active)
			SwitchToUI("warehouse.xml");
	}

	protected void OnActionCRMActivated(object sender, EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.CRM.xml");
	}

	protected void OnActionGeneralActivated(object sender, EventArgs e)
	{
		SwitchToUI("general.xml");
	}

	protected void OnActionOrganizationsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Organization));
		tdiMain.AddTab(refWin);
	}

	protected void OnSubdivisionsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<Subdivision>(),
			() => new OrmReference(typeof(Subdivision))
		);
	}

	protected void OnActionBanksRFActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Bank));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionNationalityActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Nationality));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionCitizenshipActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Citizenship));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionEmployeeActivated(object sender, EventArgs e)
	{
		var employeeFilter = new EmployeeFilterViewModel();
		employeeFilter.SetAndRefilterAtOnce(x => x.Status = EmployeeStatus.IsWorking);

		var employeeJournalFactory = new EmployeeJournalFactory(employeeFilter);

		tdiMain.AddTab(employeeJournalFactory.CreateEmployeesJournal());
	}

	protected void OnActionCarsActivated(object sender, EventArgs e)
	{
		var page = NavigationManager.OpenViewModel<CarJournalViewModel>(null);
		page.ViewModel.NavigationManager = NavigationManager;
	}

	protected void OnActionUnitsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(MeasurementUnits));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionDiscountReasonsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			() =>
			{
				return new DiscountReasonJournalViewModel(
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					new DiscountReasonRepository(),
					new ProductGroupJournalFactory(),
					new NomenclatureJournalFactory()
				);
			}
		);
	}

	protected void OnActionColorsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(EquipmentColors));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionManufacturersActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Manufacturer));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionEquipmentKindsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(() => new EquipmentKindJournalViewModel(
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices)
		);
	}

	protected void OnActionNomenclatureActivated(object sender, EventArgs e)
	{
		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		var userRepository = new UserRepository();
		var counterpartyJournalFactory = new CounterpartyJournalFactory(MainClass.AppDIContainer.BeginLifetimeScope());

		tdiMain.OpenTab(
			() => new NomenclaturesJournalViewModel(
				new NomenclatureFilterViewModel() { HidenByDefault = true },
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				VodovozGtkServicesConfig.EmployeeService,
				new NomenclatureJournalFactory(),
				counterpartyJournalFactory,
				nomenclatureRepository,
				userRepository
			));
	}

	protected void OnActionPhoneTypesActivated(object sender, EventArgs e)
	{
		IPhoneRepository phoneRepository = new PhoneRepository();

		tdiMain.AddTab(
			new PhoneTypeJournalViewModel(
				phoneRepository,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			)
		);
	}

	protected void OnActionCounterpartyHandbookActivated(object sender, EventArgs e)
	{
		CounterpartyJournalFilterViewModel filter = new CounterpartyJournalFilterViewModel() { IsForRetail = false };
		var counterpartyJournal = new CounterpartyJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);

		tdiMain.AddTab(counterpartyJournal);
	}

	protected void OnActionEMailTypesActivated(object sender, EventArgs e)
	{
		IEmailRepository emailRepository = new EmailRepository();

		tdiMain.AddTab(
			new EmailTypeJournalViewModel(
				emailRepository,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			)
		);
	}

	protected void OnActionCounterpartyPostActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Post));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionFreeRentPackageActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FreeRentPackagesJournalViewModel>(null);
	}

	protected void OnActionPaidRentPackageActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PaidRentPackagesJournalViewModel>(null);
	}

	protected void OnActionEquipmentActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(Equipment));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionDeliveryScheduleActivated(object sender, EventArgs e)
	{
		var journal = autofacScope.Resolve<DeliveryScheduleJournalViewModel>();

		journal.SelectionMode = JournalSelectionMode.None;
		tdiMain.AddTab(journal);
	}

	protected void OnActionUpdateBanksFromCBRActivated(object sender, EventArgs e)
	{
		BanksUpdater.CheckBanksUpdate(true);
	}

	protected void OnActionWarehousesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<WarehousesView>(),
			() => new WarehousesView()
		);
	}

	protected void OnActionProductSpecificationActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(ProductSpecification));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionCullingCategoryActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(CullingCategory));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionCommentTemplatesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(CommentTemplate));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionLoad1cActivated(object sender, EventArgs e)
	{
		var win = new LoadFrom1cDlg();
		tdiMain.AddTab(win);
	}

	protected void OnActionRouteColumnsActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(RouteColumn));
		tdiMain.AddTab(refWin);
	}

	protected void OnActionFuelTypeActivated(object sender, EventArgs e)
	{
		var parametersProvider = new ParametersProvider();
		var nomenclatureParametersProvider = new NomenclatureParametersProvider(parametersProvider);
		var routeListProfitabilityController = new RouteListProfitabilityController(
			new RouteListProfitabilityFactory(), nomenclatureParametersProvider,
			new ProfitabilityConstantsRepository(), new RouteListProfitabilityRepository(),
			new RouteListRepository(new StockRepository(), new BaseParametersProvider(parametersProvider)),
			new NomenclatureRepository(nomenclatureParametersProvider));
		var commonServices = ServicesConfig.CommonServices;
		var unitOfWorkFactory = UnitOfWorkFactory.GetDefaultFactory;

		var fuelTypeJournalViewModel = new SimpleEntityJournalViewModel<FuelType, FuelTypeViewModel>(
			x => x.Name,
			() => new FuelTypeViewModel(
				EntityUoWBuilder.ForCreate(), unitOfWorkFactory, commonServices, routeListProfitabilityController),
			(node) => new FuelTypeViewModel(
				EntityUoWBuilder.ForOpen(node.Id), unitOfWorkFactory, commonServices, routeListProfitabilityController),
			unitOfWorkFactory,
			commonServices);

		var fuelTypePermissionSet = commonServices.PermissionService.ValidateUserPermission(typeof(FuelType), commonServices.UserService.CurrentUserId);
		if(fuelTypePermissionSet.CanRead && !fuelTypePermissionSet.CanUpdate)
		{
			var viewAction = new JournalAction("Просмотр",
				(selected) => selected.Any(),
				(selected) => true,
				(selected) =>
				{
					var tab = fuelTypeJournalViewModel.GetTabToOpen(typeof(FuelType), selected.First().GetId());
					fuelTypeJournalViewModel.TabParent.AddTab(tab, fuelTypeJournalViewModel);
				}
			);

			(fuelTypeJournalViewModel.NodeActions as IList<IJournalAction>)?.Add(viewAction);
		}

		tdiMain.AddTab(fuelTypeJournalViewModel);
	}

	protected void OnActionDeliveryShiftActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(DeliveryShift));
		tdiMain.AddTab(refWin);
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

	protected void OnAction14Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<IncomeCategoryJournalViewModel>(null);
	}

	protected void OnAction15Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ExpenseCategoryJournalViewModel>(null);
	}

	protected void OnActionCashToggled(object sender, EventArgs e)
	{
		if(ActionCash.Active)
			SwitchToUI("cash.xml");
	}

	protected void OnActionAccountingToggled(object sender, EventArgs e)
	{
		if(ActionAccounting.Active)
			SwitchToUI("accounting.xml");
	}

	protected void OnActionDocTemplatesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DocTemplate>(),
			() => new OrmReference(typeof(DocTemplate))
		);
	}

	protected void OnActionToolBarTextToggled(object sender, EventArgs e)
	{
		if(ActionToolBarText.Active)
			ToolBarMode(ToolbarStyle.Text);
	}

	private void ToolBarMode(ToolbarStyle style)
	{
		if(CurrentUserSettings.Settings.ToolbarStyle != style)
		{
			CurrentUserSettings.Settings.ToolbarStyle = style;
			CurrentUserSettings.SaveSettings();
		}
		toolbarMain.ToolbarStyle = (Gtk.ToolbarStyle)style;
		tlbComplaints.ToolbarStyle = (Gtk.ToolbarStyle)style;
		ActionIconsExtraSmall.Sensitive = ActionIconsSmall.Sensitive = ActionIconsMiddle.Sensitive = ActionIconsLarge.Sensitive =
			style != ToolbarStyle.Text;
	}

	private void ToolBarMode(IconsSize size)
	{
		if(CurrentUserSettings.Settings.ToolBarIconsSize != size)
		{
			CurrentUserSettings.Settings.ToolBarIconsSize = size;
			CurrentUserSettings.SaveSettings();
		}
		switch(size)
		{
			case IconsSize.ExtraSmall:
				toolbarMain.IconSize = IconSize.SmallToolbar;
				tlbComplaints.IconSize = IconSize.SmallToolbar;
				break;
			case IconsSize.Small:
				toolbarMain.IconSize = IconSize.LargeToolbar;
				tlbComplaints.IconSize = IconSize.LargeToolbar;
				break;
			case IconsSize.Middle:
				toolbarMain.IconSize = IconSize.Dnd;
				tlbComplaints.IconSize = IconSize.Dnd;
				break;
			case IconsSize.Large:
				toolbarMain.IconSize = IconSize.Dialog;
				tlbComplaints.IconSize = IconSize.Dialog;
				break;
		}
	}

	protected void OnActionToolBarIconToggled(object sender, EventArgs e)
	{
		if(ActionToolBarIcon.Active)
			ToolBarMode(ToolbarStyle.Icons);
	}

	protected void OnActionToolBarBothToggled(object sender, EventArgs e)
	{
		if(ActionToolBarBoth.Active)
			ToolBarMode(ToolbarStyle.Both);
	}

	protected void OnActionIconsExtraSmallToggled(object sender, EventArgs e)
	{
		if(ActionIconsExtraSmall.Active)
			ToolBarMode(IconsSize.ExtraSmall);
	}

	protected void OnActionIconsSmallToggled(object sender, EventArgs e)
	{
		if(ActionIconsSmall.Active)
			ToolBarMode(IconsSize.Small);
	}

	protected void OnActionIconsMiddleToggled(object sender, EventArgs e)
	{
		if(ActionIconsMiddle.Active)
			ToolBarMode(IconsSize.Middle);
	}

	protected void OnActionIconsLargeToggled(object sender, EventArgs e)
	{
		if(ActionIconsLarge.Active)
			ToolBarMode(IconsSize.Large);
	}

	protected void OnActionDeliveryPointsActivated(object sender, EventArgs e)
	{
		var dpJournalFactory = new DeliveryPointJournalFactory();
		var deliveryPointJournal = dpJournalFactory.CreateDeliveryPointJournal();
		tdiMain.AddTab(deliveryPointJournal);
	}

	protected void OnPropertiesActionActivated(object sender, EventArgs e)
	{
		var subdivisionJournalFactory = new SubdivisionJournalFactory();
		var subdivisionRepository = new SubdivisionRepository(new ParametersProvider());
		var counterpartyJournalFactory = new CounterpartyJournalFactory(MainClass.AppDIContainer.BeginLifetimeScope());

		tdiMain.OpenTab(
			() => new UserSettingsViewModel(
				EntityUoWBuilder.ForOpen(CurrentUserSettings.Settings.Id),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				VodovozGtkServicesConfig.EmployeeService,
				new SubdivisionParametersProvider(new ParametersProvider()),
				subdivisionJournalFactory,
				counterpartyJournalFactory,
				subdivisionRepository,
				new NomenclaturePricesRepository()
			));
	}

	protected void OnActionTransportationWagonActivated(object sender, EventArgs e)
	{
		var movingWagonFilter = new MovementWagonJournalFilterViewModel();
		var movingWagonJournal = new MovementWagonJournalViewModel(movingWagonFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
		tdiMain.AddTab(movingWagonJournal);
	}

	protected void OnActionRegrandingOfGoodsTempalteActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<RegradingOfGoodsTemplate>(),
			() => new OrmReference(typeof(RegradingOfGoodsTemplate))
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

	protected void OnActionArchiveToggled(object sender, EventArgs e)
	{
		if(ActionArchive.Active)
			SwitchToUI("archive.xml");
	}

	protected void OnActionStaffToggled(object sender, EventArgs e)
	{
		if(ActionStaff.Active)
			SwitchToUI("Vodovoz.toolbars.staff.xml");
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

	protected void OnActionFineCommentTemplatesActivated(object sender, EventArgs e)
	{
		OrmReference refWin = new OrmReference(typeof(FineTemplate));
		tdiMain.AddTab(refWin);
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
				new CounterpartyJournalFactory(MainClass.AppDIContainer.BeginLifetimeScope()),
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

	protected void OnActionDeliveryDayScheduleActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DeliveryDaySchedule>(),
			() => new OrmReference(typeof(DeliveryDaySchedule))
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

	protected void OnActionHistoryLogActivated(object sender, EventArgs e)
	{
		tdiMain.AddTab(new Vodovoz.Dialogs.HistoryView(new UserJournalFactory()));
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

	protected void OnAction47Activated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PremiumTemplateJournalViewModel>(null, OpenPageOptions.IgnoreHash);
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

	protected void OnActionFolders1cActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<Folder1c>(),
			() => new OrmReference(typeof(Folder1c))
		);
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

	protected void OnActionCameFromActivated(object sender, EventArgs e)
	{
		ClientCameFromFilterViewModel filter = new ClientCameFromFilterViewModel()
		{
			HidenByDefault = true
		};
		var journal = new ClientCameFromJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
		tdiMain.AddTab(journal);
	}

	protected void OnActionOrganizationOwnershipTypeActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<OrganizationOwnershipTypeJournalViewModel>(null);
	}

	protected void OnActionProductGroupsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			TdiTabBase.GenerateHashName<ProductGroupView>(),
			() => new ProductGroupView()
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

	protected void OnActionDeliveryPriceRulesActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<DeliveryPriceRuleJournalViewModel>(null);
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

	protected void OnActionCertificatesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<Certificate>(),
			() => new OrmReference(typeof(Certificate))
		);
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

	protected void OnActionTariffZonesActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(() => new TariffZoneJournalViewModel(
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices)
		);
	}

	protected void OnActionStockMovementsAdvancedReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<StockMovementsAdvancedReport>(),
			() => new QSReport.ReportViewDlg(new StockMovementsAdvancedReport())
		);
	}

	protected void OnActionNonReturnReasonsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<NonReturnReason>(),
			() => new OrmReference(typeof(NonReturnReason))
		);
	}

	protected void OnActionPromotionalSetsActivated(object sender, EventArgs e)
	{
		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		var userRepository = new UserRepository();

		var counterpartyJournalFactory = new CounterpartyJournalFactory(MainClass.AppDIContainer.BeginLifetimeScope());

		tdiMain.AddTab(
			new PromotionalSetsJournalViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				VodovozGtkServicesConfig.EmployeeService,
				counterpartyJournalFactory,
				new NomenclatureJournalFactory(),
				nomenclatureRepository,
				userRepository
			)
		);
	}

	protected void OnActionDeliveryPointCategoryActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DeliveryPointCategory>(),
			() => new OrmReference(typeof(DeliveryPointCategory))
		);
	}

	protected void OnActionCounterpartyActivityKindsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<CounterpartyActivityKind>(),
			() => new OrmReference(typeof(CounterpartyActivityKind))
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

	protected void OnActionPaymentsFromActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<PaymentsFromJournalViewModel>(null);
	}

	protected void OnAction62Activated(object sender, EventArgs e)
	{
		var widget = new ResendEmailsDialog();
		tdiMain.AddTab(widget);
	}

	protected void OnActionComplaintSourcesActivated(object sender, EventArgs e)
	{
		var complaintSourcesViewModel = new SimpleEntityJournalViewModel<ComplaintSource, ComplaintSourceViewModel>(
			x => x.Name,
			() => new ComplaintSourceViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices),
			(node) => new ComplaintSourceViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices),
			 UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices
		);
		tdiMain.AddTab(complaintSourcesViewModel);
	}

	protected void OnActionSuppliersActivated(object sender, EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.suppliers.xml");
	}

	protected void OnActionPlanImplementationReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PlanImplementationReport>(),
			() => new QSReport.ReportViewDlg(new PlanImplementationReport())
		);
	}

	protected void OnActionWageDistrictActivated(object sender, EventArgs e)
	{
		tdiMain.AddTab(
			new WageDistrictsJournalViewModel(
				 UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			)
		);
	}

	protected void OnActionRatesActivated(object sender, EventArgs e)
	{
		tdiMain.AddTab(
			new WageDistrictLevelRatesJournalViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			)
		);
	}

	protected void OnActionSalesPlansActivated(object sender, EventArgs e)
	{
		tdiMain.AddTab(
			new SalesPlanJournalViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new NomenclatureJournalFactory()
			)
		);
	}

	protected void OnActionZeroDebtClientReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ZeroDebtClientReport>(),
			() => new QSReport.ReportViewDlg(new ZeroDebtClientReport())
		);
	}

	protected void OnActionComplaintKindActivated(object sender, EventArgs e)
	{
		var employeeJournalFactory = new EmployeeJournalFactory();
		var salesPlanJournalFactory = new SalesPlanJournalFactory();
		var nomenclatureSelectorFactory = new NomenclatureJournalFactory();

		tdiMain.OpenTab(() => new ComplaintKindJournalViewModel(
			new ComplaintKindJournalFilterViewModel
			{
				HidenByDefault = true
			},
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			employeeJournalFactory,
			salesPlanJournalFactory,
			nomenclatureSelectorFactory,
			autofacScope.BeginLifetimeScope())
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

	protected void OnActionUndeliveryProblemSourcesActivated(object sender, EventArgs e)
	{
		var undeliveryProblemSourcesViewModel = new SimpleEntityJournalViewModel<UndeliveryProblemSource, UndeliveryProblemSourceViewModel>(
			x => x.Name,
			() => new UndeliveryProblemSourceViewModel(
				EntityUoWBuilder.ForCreate(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			),
			(node) => new UndeliveryProblemSourceViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			),
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices
		);
		undeliveryProblemSourcesViewModel.SetActionsVisible(deleteActionEnabled: false);
		tdiMain.AddTab(undeliveryProblemSourcesViewModel);
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
				new PaymentsFromBankClientReport(new CounterpartyJournalFactory(MainClass.AppDIContainer.BeginLifetimeScope()), new UserRepository(), ServicesConfig.CommonServices))
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
		var employeeJournalFactory = new EmployeeJournalFactory();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ChainStoreDelayReport>(),
			() => new QSReport.ReportViewDlg(new ChainStoreDelayReport(employeeJournalFactory))
		);
	}

	protected void OnActionReturnedTareReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ReturnedTareReport>(),
			() => new QSReport.ReportViewDlg(new ReturnedTareReport(ServicesConfig.InteractiveService))
		);
	}

	protected void OnActionReturnTareReasonsActivated(object sender, EventArgs e)
	{
		tdiMain.AddTab(
			new ReturnTareReasonsJournalViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			)
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

	protected void OnActionReturnTareReasonCategoriesActivated(object sender, EventArgs e)
	{
		tdiMain.AddTab(
			new ReturnTareReasonCategoriesJournalViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			)
		);
	}

	protected void OnActionLateArrivalReasonsActivated(object sender, EventArgs e)
	{
		tdiMain.AddTab(
			new LateArrivalReasonsJournalViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			)
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

	protected void OnActionRetailActivated(object sender, EventArgs e)
	{
		if(ActionRetail.Active)
			SwitchToUI("retail.xml");
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
		var counterpartyJournalFactory = new CounterpartyJournalFactory(MainClass.AppDIContainer.BeginLifetimeScope());
		var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
		var employeeJournalFactory = new EmployeeJournalFactory();

		var orderJournalFilter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, employeeJournalFactory)
		{
			IsForRetail = true
		};
		NavigationManager.OpenViewModel<RetailOrderJournalViewModel, OrderJournalFilterViewModel>(null, orderJournalFilter);
	}

	protected void OnActionSalesChannelsJournalActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			() => new SalesChannelJournalViewModel(
					new SalesChannelJournalFilterViewModel(),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices
			)
		);
	}

	protected void OnActionResponsiblePersonTypesJournalActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			() => new DeliveryPointResponsiblePersonTypeJournalViewModel(
					new DeliveryPointResponsiblePersonTypeJournalFilterViewModel(),
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices
			)
		);
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

	protected void OnReorderTabsToggled(object sender, EventArgs e)
	{
		var isActive = ReorderTabs.Active;
		if(CurrentUserSettings.Settings.ReorderTabs != isActive)
		{
			CurrentUserSettings.Settings.ReorderTabs = isActive;
			CurrentUserSettings.SaveSettings();
			MessageDialogHelper.RunInfoDialog("Изменения вступят в силу после перезапуска программы");
		}
	}

	string[] GetTabsColors() =>
		new[] { "#F81919", "#009F6B", "#1F8BFF", "#FF9F00", "#FA7A7A", "#B46034", "#99B6FF", "#8F2BE1", "#00CC44" };

	protected void OnHighlightTabsWithColorToggled(object sender, EventArgs e)
	{
		var isActive = HighlightTabsWithColor.Active;
		if(!isActive)
			KeepTabColor.Active = false;
		KeepTabColor.Sensitive = isActive;
		if(CurrentUserSettings.Settings.HighlightTabsWithColor != isActive)
		{
			CurrentUserSettings.Settings.HighlightTabsWithColor = isActive;
			CurrentUserSettings.SaveSettings();
			MessageDialogHelper.RunInfoDialog("Изменения вступят в силу после перезапуска программы");
		}
	}

	protected void OnKeepTabColorToggled(object sender, EventArgs e)
	{
		var isActive = KeepTabColor.Active;
		if(CurrentUserSettings.Settings.KeepTabColor != isActive)
		{
			CurrentUserSettings.Settings.KeepTabColor = isActive;
			CurrentUserSettings.SaveSettings();
			MessageDialogHelper.RunInfoDialog("Изменения вступят в силу после перезапуска программы");
		}
	}

	protected void OnActionNomenclaturePlanActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(() => new NomenclaturesPlanJournalViewModel(
					new NomenclaturePlanFilterViewModel() { HidenByDefault = true },
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices)
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

	protected void OnActionCarServiceAcivated(object sender, EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.car_service.xml");
	}

	protected void OnActionCarEventTypeActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(() => new CarEventTypeJournalViewModel(
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices)
		);
	}

	protected void OnActionDriversComplaintReasonsJournalActivated(object sender, EventArgs e)
	{
		var driversComplaintReasonsFilter = new DriverComplaintReasonJournalFilterViewModel();
		var driversComplaintReasonsJournal = new DriverComplaintReasonsJournalViewModel(driversComplaintReasonsFilter,
			UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
		tdiMain.AddTab(driversComplaintReasonsJournal);
	}

	protected void OnActionComplaintObjectActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(() => new ComplaintObjectJournalViewModel(
			new ComplaintObjectJournalFilterViewModel()
			{
				HidenByDefault = true
			},
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices)
		);
	}

	protected void OnActionFlyersActivated(object sender, EventArgs e)
	{
		var journal = new FlyersJournalViewModel(
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			new NomenclatureJournalFactory(),
			new FlyerRepository());

		tdiMain.AddTab(journal);
	}

	protected void OnActionUndeliveryTransferAbsenceReasonActivated(object sender, EventArgs e)
	{
		var filterViewModel = new UndeliveryTransferAbsenceReasonJournalFilterViewModel();

		var journal = new UndeliveryTransferAbsenceReasonJournalViewModel(
			filterViewModel,
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices);

		tdiMain.AddTab(journal);
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

	protected void OnActionCarManufacturersActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CarManufacturerJournalViewModel>(null);
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

	protected void OnGeneralSettingsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<GeneralSettingsViewModel>(null);
	}

	protected void OnComplaintResultsOfCounterpartyActionActivated(object sender, EventArgs e)
	{
		var complaintResultsOfCounterpartyViewModel =
			new ComplaintResultsOfCounterpartyJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
		tdiMain.AddTab(complaintResultsOfCounterpartyViewModel);
	}

	protected void OnComplaintResultsOfEmployeesActionActivated(object sender, EventArgs e)
	{
		var complaintResultsOfEmployeesViewModel =
			new ComplaintResultsOfEmployeesJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
		tdiMain.AddTab(complaintResultsOfEmployeesViewModel);
	}

	protected void OnActionCounterpartyCashlessDebtsReportActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CounterpartyCashlessDebtsReport>(),
			() => new QSReport.ReportViewDlg(autofacScope.Resolve<CounterpartyCashlessDebtsReport>())
		);
	}

	protected void OnActionAdditionalLoadSettingsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<AdditionalLoadingSettingsViewModel>(null);
	}

	protected void OnActionRoboAtsCounterpartyNameActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RoboAtsCounterpartyNameJournalViewModel>(null);
	}

	protected void OnActionRoboAtsCounterpartyPatronymicActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RoboAtsCounterpartyPatronymicJournalViewModel>(null);
	}

	protected void OnActionCarModelsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<CarModelJournalViewModel>(null);
	}

	protected void OnRoboatsExportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<RoboatsCatalogExportViewModel>(null);
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

	protected void OnUnsubscribingReasonsActionActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(() => new BulkEmailEventReasonJournalViewModel(
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices));
	}

	protected void OnActionBulkEmailEventsReportActivated(object sender, EventArgs e)
	{
		ICounterpartyJournalFactory counterpartyJournalFactory = new CounterpartyJournalFactory(MainClass.AppDIContainer.BeginLifetimeScope());
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

	protected void OnProfitabilityConstantsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ProfitabilityConstantsViewModel, IValidator>(
			null, ServicesConfig.ValidationService, OpenPageOptions.IgnoreHash);
	}

	private void ActionGroupPricingActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<NomenclatureGroupPricingViewModel>(null);
	}

	protected void OnActionSalesDepartmentAcivated(System.Object sender, System.EventArgs e)
	{
		SwitchToUI("Vodovoz.toolbars.sales_department.xml");
	}

	protected void OnActionResponsibleActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ResponsibleJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnActionEdoOperatorsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EdoOperatorsJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnUsersRolesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<UserRolesJournalViewModel>(null);
	}

	protected void OnEmployeeRegistrationsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<EmployeeRegistrationsJournalViewModel>(null);
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

	protected void OnActionComplaintDetalizationJournalActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ComplaintDetalizationJournalViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnSalesBySubdivisionsAnalitycsActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<SalesBySubdivisionsAnalitycsReportViewModel>(null, OpenPageOptions.IgnoreHash);
	}

	protected void OnExternalCounterpartiesMatchingActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<ExternalCounterpartiesMatchingJournalViewModel>(null);
	}

	protected void OnInventoryInstancesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<InventoryInstancesJournalViewModel>(null);
	}

	private DateTime GetDateTimeFGromVersion(Version version) =>
		new DateTime(2000, 1, 1)
			.AddDays(version.Build)
			.AddSeconds(version.Revision * 2);

	protected void OnInventoryInstanceMovementReportActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<InventoryInstanceMovementReportViewModel>(null);
	}

	protected void OnInventoryNomenclaturesActionActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<InventoryNomenclaturesJournalViewModel>(null);
	}

	protected void OnActionFinancialCategoriesGroupsActivated(object sender, EventArgs e)
	{
		NavigationManager.OpenViewModel<FinancialCategoriesGroupsJournalViewModel>(null);
	}
}
