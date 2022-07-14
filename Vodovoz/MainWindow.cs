using Autofac;
using Fias.Service;
using Gtk;
using MySql.Data.MySqlClient;
using NetTopologySuite.Operation.OverlayNG;
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
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Vodovoz;
using Vodovoz.Core;
using Vodovoz.Core.DataService;
using Vodovoz.Core.Journal;
using Vodovoz.Dialogs.OnlineStore;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Retail;
using Vodovoz.Domain.Sale;
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
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using Vodovoz.JournalSelector;
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
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Accounting;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs.Counterparty;
using Vodovoz.ViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels;
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
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Proposal;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;
using Vodovoz.ViewModels.Journals.JournalViewModels.Retail;
using Vodovoz.ViewModels.Journals.JournalViewModels.Security;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.ReportsParameters.Cash;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.Users;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.ViewModels.ViewModels.Settings;
using Vodovoz.ViewWidgets;
using VodovozInfrastructure.Configuration;
using VodovozInfrastructure.Interfaces;
using VodovozInfrastructure.Passwords;
using Connection = QS.Project.DB.Connection;
using ToolbarStyle = Vodovoz.Domain.Employees.ToolbarStyle;
using UserRepository = Vodovoz.EntityRepositories.UserRepository;
using QS.Project.Services.FileDialog;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.Entity;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.ViewModels.Reports.FastDelivery;
using Vodovoz.ViewModels.Dialogs.Roboats;
using QS.DomainModel.NotifyChange;

public partial class MainWindow : Gtk.Window
{
	private static Logger logger = LogManager.GetCurrentClassLogger();
	private uint lastUiId;
	private readonly ILifetimeScope autofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
	private readonly IApplicationInfo applicationInfo;
	private readonly IPasswordValidator passwordValidator;
	private readonly IApplicationConfigurator applicationConfigurator;

	public TdiNotebook TdiMain => tdiMain;
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

		Title = $"{applicationInfo.ProductTitle} v{applicationInfo.Version} от {applicationInfo.BuildDate:dd.MM.yyyy HH:mm}";
		//Настраиваем модули
		ActionUsers.Sensitive = QSMain.User.Admin;
		ActionAdministration.Sensitive = QSMain.User.Admin;
		labelUser.LabelProp = QSMain.User.Name;
		var cashier = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("role_сashier");
		ActionCash.Sensitive = ActionIncomeBalanceReport.Sensitive = ActionCashBook.Sensitive = cashier;
		ActionAccounting.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("money_manage_bookkeeping");
		ActionRouteListsAtDay.Sensitive =
			ActionRouteListTracking.Sensitive =
			ActionRouteListMileageCheck.Sensitive =
			ActionRouteListAddressesTransferring.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
		ActionStock.Sensitive = CurrentPermissions.Warehouse.Allowed().Any();

		bool hasAccessToCRM = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("access_to_crm");
		bool hasAccessToSalaries = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("access_to_salaries");
		bool hasAccessToWagesAndBonuses = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("access_to_fines_bonuses");
		ActionEmployeesBonuses.Sensitive = hasAccessToWagesAndBonuses; //Премии сотрудников
		ActionEmployeeFines.Sensitive = hasAccessToWagesAndBonuses; //Штрафы сотрудников
		ActionDriverWages.Sensitive = hasAccessToSalaries; //Зарплаты водителей
		ActionWagesOperations.Sensitive = hasAccessToSalaries; //Зарплаты сотрудников
		ActionForwarderWageReport.Sensitive = hasAccessToSalaries; //Зарплаты экспедиторов
		ActionDriversWageBalance.Visible = hasAccessToSalaries; //Баланс водителей
		ActionCRM.Sensitive = hasAccessToCRM;

		bool canEditWage = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_wage");
		ActionWageDistrict.Sensitive = canEditWage;
		ActionRates.Sensitive = canEditWage;

		bool canEditWageBySelfSubdivision = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_wage_by_self_subdivision");
		ActionSalesPlans.Sensitive = canEditWageBySelfSubdivision;

		ActionFinesJournal.Visible = ActionPremiumJournal.Visible = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("access_to_fines_bonuses");
		ActionReports.Sensitive = false;
		//ActionServices.Visible = false;
		ActionDocTemplates.Visible = QSMain.User.Admin;
		ActionService.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("database_maintenance");
		ActionEmployeeWorkChart.Sensitive = false;

		//Скрываем справочник стажеров
		ActionTrainee.Visible = false;

		ActionAddOrder.Sensitive = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(typeof(Order), QSMain.User.Id)?.CanCreate ?? false;
		ActionExportImportNomenclatureCatalog.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");
		ActionDistricts.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DistrictsSet)).CanRead;

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

		#region Пользователь с правом работы только со складом и рекламациями

		bool accessToWarehouseAndComplaints;

		using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			accessToWarehouseAndComplaints =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !ServicesConfig.CommonServices.UserService.GetCurrentUser(uow).IsAdmin;
		}

		menubarMain.Visible = ActionOrders.Visible = ActionServices.Visible = ActionLogistics.Visible = ActionCash.Visible =
			ActionAccounting.Visible = ActionReports.Visible = ActionArchive.Visible = ActionStaff.Visible = ActionCRM.Visible =
				ActionSuppliers.Visible = ActionCashRequest.Visible = ActionRetail.Visible = ActionCarService.Visible =
					MangoAction.Visible = !accessToWarehouseAndComplaints;

		#endregion

		BanksUpdater.CheckBanksUpdate(false);

		// Блокировка отчетов для торговых представителей

		bool userIsSalesRepresentative;

		using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
		{
			userIsSalesRepresentative = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_is_sales_representative")
			&& !ServicesConfig.CommonServices.UserService.GetCurrentUser(uow).IsAdmin;
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

		var userCanManageRegisteredRMs = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_can_manage_registered_rms");

		registeredRMAction.Visible = userCanManageRegisteredRMs;

		// Настройки розницы

		var userHaveAccessToRetail = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_to_retail");

		ActionRetail.Sensitive = userHaveAccessToRetail;

		ActionRetailUndeliveredOrdersJournal.Sensitive = false; // Этот журнал не готов - выключено до реализации фичи

		ActionAdditionalLoadSettings.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService
			.ValidateEntityPermission(typeof(AdditionalLoadingNomenclatureDistribution)).CanRead;

	}

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
		var counterpartyJournalFactory = new CounterpartyJournalFactory();

		IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
			new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				new NomenclatureFilterViewModel(), counterpartyJournalFactory, nomenclatureRepository, userRepository);

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
		var commonServices = ServicesConfig.CommonServices;
		var unitOfWorkFactory = UnitOfWorkFactory.GetDefaultFactory;

		var fuelTypeJournalViewModel = new SimpleEntityJournalViewModel<FuelType, FuelTypeViewModel>(
			x => x.Name,
			() => new FuelTypeViewModel(EntityUoWBuilder.ForCreate(), unitOfWorkFactory, commonServices),
			(node) => new FuelTypeViewModel(EntityUoWBuilder.ForOpen(node.Id), unitOfWorkFactory, commonServices),
			QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
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
		var incomeCategoryFilter = new IncomeCategoryJournalFilterViewModel();
		IFileChooserProvider chooserProvider = new Vodovoz.FileChooser();
		var employeeJournalFactory = new EmployeeJournalFactory();
		var subdivisionJournalFactory = new SubdivisionJournalFactory();
		var incomeFactory = new IncomeCategorySelectorFactory();

		tdiMain.AddTab(
			new IncomeCategoryJournalViewModel(
				incomeCategoryFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				chooserProvider,
				employeeJournalFactory,
				subdivisionJournalFactory,
				incomeFactory
			)
		);
	}

	protected void OnAction15Activated(object sender, EventArgs e)
	{
		var expenseCategoryFilter = new ExpenseCategoryJournalFilterViewModel();
		IFileChooserProvider chooserProvider = new Vodovoz.FileChooser();
		var employeeJournalFactory = new EmployeeJournalFactory();
		var subdivisionJournalFactory = new SubdivisionJournalFactory();
		var expenseFactory = new ExpenseCategorySelectorFactory();

		tdiMain.AddTab(
			new ExpenseCategoryJournalViewModel(
				expenseCategoryFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				chooserProvider,
				employeeJournalFactory,
				subdivisionJournalFactory,
				expenseFactory
			)
		);
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
		var counterpartyJournalFactory = new CounterpartyJournalFactory();

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
				new NomenclatureFixedPriceRepository()
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
		ISubdivisionJournalFactory subdivisionJournalFactory = new SubdivisionJournalFactory();

		IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener = new UndeliveredOrdersJournalOpener();

		var parametersProvider = new ParametersProvider();
		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(parametersProvider));
		var employeeJournalFactory = new EmployeeJournalFactory();
		var userRepository = new UserRepository();

		ICounterpartyJournalFactory counterpartySelectorFactory = new CounterpartyJournalFactory();

		ISubdivisionRepository subdivisionRepository = new SubdivisionRepository(parametersProvider);
		IRouteListItemRepository routeListItemRepository = new RouteListItemRepository();
		IFileDialogService fileDialogService = new FileDialogService();

		var journal = new ComplaintsJournalViewModel(
			UnitOfWorkFactory.GetDefaultFactory,
			ServicesConfig.CommonServices,
			undeliveredOrdersJournalOpener,
			VodovozGtkServicesConfig.EmployeeService,
			counterpartySelectorFactory,
			routeListItemRepository,
			new SubdivisionParametersProvider(new ParametersProvider()),
			new ComplaintFilterViewModel(
				ServicesConfig.CommonServices,
				subdivisionRepository,
				employeeJournalFactory,
				counterpartySelectorFactory
			)
			{
				HidenByDefault = true
			},
			fileDialogService,
			subdivisionRepository,
			new GtkReportViewOpener(),
			new GtkTabsOpener(),
			nomenclatureRepository,
			userRepository,
			new OrderSelectorFactory(),
			employeeJournalFactory,
			counterpartySelectorFactory,
			new DeliveryPointJournalFactory(),
			subdivisionJournalFactory,
			new SalesPlanJournalFactory(),
			new NomenclatureJournalFactory(),
			new EmployeeSettings(new ParametersProvider()),
			new UndeliveredOrdersRepository()
		);

		tdiMain.AddTab(journal);
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
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.WagesOperationsReport>(),
			() => new QSReport.ReportViewDlg(new Vodovoz.Reports.WagesOperationsReport())
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
		IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
			new DefaultEntityAutocompleteSelectorFactory<Counterparty, RetailCounterpartyJournalViewModel,
				CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);

		var employeeJournalFactory = new EmployeeJournalFactory();

		IEntityAutocompleteSelectorFactory salesChannelselectorFactory =
			new DefaultEntityAutocompleteSelectorFactory<SalesChannel, SalesChannelJournalViewModel,
				SalesChannelJournalFilterViewModel>(ServicesConfig.CommonServices);

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<QualityReport>(),
			() => new QSReport.ReportViewDlg(new QualityReport(counterpartySelectorFactory, salesChannelselectorFactory,
				employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory(), UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.InteractiveService)));
	}

	protected void OnActionRoutesListRegisterActivated(object sender, EventArgs e) => OpenDriverRoutesListRegisterReport();
	protected void OnActionOrderedByIdRoutesListRegisterActivated(object sender, EventArgs e) => OpenRoutesListRegisterReport();
	protected void OnActionProducedProductionReportActivated(object sender, EventArgs e)
	{
		#region DependencyCreation

		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
		var userRepository = new UserRepository();

		IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
			new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				new NomenclatureFilterViewModel(), new CounterpartyJournalFactory(), nomenclatureRepository, userRepository);

		#endregion

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<ProducedProductionReport>(),
			() => new QSReport.ReportViewDlg(
				new ProducedProductionReport(nomenclatureSelectorFactory))
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
		IFiasApiClient fiasApiClient = new FiasApiClient(fiasApiParametersProvider.FiasApiBaseUrl, fiasApiParametersProvider.FiasApiToken);

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
		tdiMain.AddTab(new Vodovoz.Dialogs.HistoryView());
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
		bool right = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_delivery_price_rules");
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<DeliveryPriceRule>(),
			() =>
			{
				var dlg = new OrmReference(typeof(DeliveryPriceRule))
				{
					ButtonMode = right ? ReferenceButtonMode.CanAll : ReferenceButtonMode.None
				};
				return dlg;
			}
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
		UsersDialog usersDlg = new UsersDialog(ServicesConfig.InteractiveService);
		usersDlg.Show();
		usersDlg.Run();
		usersDlg.Destroy();
	}

	protected void OnActionGeographicGroupsActivated(object sender, EventArgs e)
	{
		tdiMain.OpenTab(
			OrmReference.GenerateHashName<GeographicGroup>(),
			() => new OrmReference(typeof(GeographicGroup))
		);
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
		var districtFilter = new DistrictJournalFilterViewModel { Status = DistrictsSetStatus.Active };

		var districtSelectorFactory = new EntityAutocompleteSelectorFactory<DistrictJournalViewModel>(typeof(District),
			() => new DistrictJournalViewModel(districtFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices));

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<FirstClientsReport>(),
			() => new QSReport.ReportViewDlg(new FirstClientsReport(districtSelectorFactory, new DiscountReasonRepository()))
		);
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

		var counterpartyJournalFactory = new CounterpartyJournalFactory();

		IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
			new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				new NomenclatureFilterViewModel(), counterpartyJournalFactory, nomenclatureRepository, userRepository);

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
			employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory(),
			salesPlanJournalFactory,
			nomenclatureSelectorFactory)
		);
	}

	protected void OnActionSetBillsReportActivated(object sender, EventArgs e)
	{
		var subdivisionJournalFactory = new SubdivisionJournalFactory();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<SetBillsReport>(),
			() => new QSReport.ReportViewDlg(new SetBillsReport(
				UnitOfWorkFactory.GetDefaultFactory,
				subdivisionJournalFactory.CreateSubdivisionAutocompleteSelectorFactory()))
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
				new WayBillReport(
					autofacScope.Resolve<IEmployeeJournalFactory>(),
					autofacScope.Resolve<ICarJournalFactory>()
				)
			)
		);
	}

	protected void OnActionPaymentsReportActivated(object sender, EventArgs e)
	{
		var counterpartyAutocompleteSelectorFactory =
			new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);

		var userRepository = new UserRepository();

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<PaymentsFromBankClientReport>(),
			() => new QSReport.ReportViewDlg(
				new PaymentsFromBankClientReport(counterpartyAutocompleteSelectorFactory, userRepository, ServicesConfig.CommonServices))
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

		var page = NavigationManager.OpenViewModel<PayoutRequestsJournalViewModel, IEmployeeJournalFactory, bool>
			(null, employeeJournalFactory, false, OpenPageOptions.IgnoreHash);
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
		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<OrderChangesReport>(),
			() => new QSReport.ReportViewDlg(new OrderChangesReport(new ReportDefaultsProvider(new ParametersProvider())))
		);
	}

	protected void OnRegisteredRMActionActivated(object sender, EventArgs e)
	{
		tdiMain.AddTab(
			new RegisteredRMJournalViewModel(
				new RegisteredRMJournalFilterViewModel(),
				UnitOfWorkFactory.GetDefaultFactory,
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
		ISubdivisionJournalFactory subdivisionJournalFactory = new SubdivisionJournalFactory();

		IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener = new UndeliveredOrdersJournalOpener();

		var parametersProvider = new ParametersProvider();
		var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(parametersProvider));
		var employeeJournalFactory = new EmployeeJournalFactory();
		var userRepository = new UserRepository();

		ICounterpartyJournalFactory counterpartySelectorFactory = new CounterpartyJournalFactory();

		ISubdivisionRepository subdivisionRepository = new SubdivisionRepository(new ParametersProvider());
		IRouteListItemRepository routeListItemRepository = new RouteListItemRepository();
		IFileDialogService fileDialogService = new FileDialogService();

		tdiMain.OpenTab(
			() =>
			{
				return new ComplaintsJournalViewModel(
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					undeliveredOrdersJournalOpener,
					VodovozGtkServicesConfig.EmployeeService,
					counterpartySelectorFactory,
					routeListItemRepository,
					new SubdivisionParametersProvider(new ParametersProvider()),
					new ComplaintFilterViewModel(
						ServicesConfig.CommonServices,
						subdivisionRepository,
						employeeJournalFactory,
						counterpartySelectorFactory
					)
					{ IsForRetail = true },
					fileDialogService,
					subdivisionRepository,
					new GtkReportViewOpener(),
					new GtkTabsOpener(),
					nomenclatureRepository,
					userRepository,
					new OrderSelectorFactory(),
					employeeJournalFactory,
					counterpartySelectorFactory,
					new DeliveryPointJournalFactory(),
					subdivisionJournalFactory,
					new SalesPlanJournalFactory(),
					new NomenclatureJournalFactory(),
					new EmployeeSettings(new ParametersProvider()),
					new UndeliveredOrdersRepository()
				);
			}
		);
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
		var counterpartyJournalFactory = new CounterpartyJournalFactory();
		var deliveryPointJournalFactory = new DeliveryPointJournalFactory();

		var orderJournalFilter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory)
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
		IEntityAutocompleteSelectorFactory carEntityAutocompleteSelectorFactory
			= new EntityAutocompleteSelectorFactory<CarJournalViewModel>(typeof(Car),
				() =>
				{
					var filter = new CarJournalFilterViewModel(new CarModelJournalFactory())
					{
						Archive = false,
						VisitingMasters = false,
						RestrictedCarTypesOfUse = new List<CarTypeOfUse>(new[] { CarTypeOfUse.Largus, CarTypeOfUse.GAZelle })
					};
					filter.SetFilterSensitivity(false);
					filter.CanChangeRestrictedCarOwnTypes = true;
					return new CarJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices);
				}
			);

		var uowFactory = autofacScope.Resolve<IUnitOfWorkFactory>();
		var interactiveService = autofacScope.Resolve<IInteractiveService>();

		var viewModel = new CarsExploitationReportViewModel(
			uowFactory, interactiveService, NavigationManager, new BaseParametersProvider(new ParametersProvider()),
			carEntityAutocompleteSelectorFactory);

		tdiMain.AddTab(viewModel);
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
		IEntityAutocompleteSelectorFactory districtSelectorFactory =
			new DefaultEntityAutocompleteSelectorFactory<District, DistrictJournalViewModel,
				DistrictJournalFilterViewModel>(ServicesConfig.CommonServices);

		IEntityAutocompleteSelectorFactory salesChannelselectorFactory =
			new DefaultEntityAutocompleteSelectorFactory<SalesChannel, SalesChannelJournalViewModel,
				SalesChannelJournalFilterViewModel>(ServicesConfig.CommonServices);

		tdiMain.OpenTab(
			QSReport.ReportViewDlg.GenerateHashName<CounterpartyReport>(),
			() => new QSReport.ReportViewDlg(new CounterpartyReport(salesChannelselectorFactory, districtSelectorFactory,
				UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.InteractiveService)));
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
				employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory(),
				employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory(), ServicesConfig.InteractiveService))
		);
	}

	protected void OnActionDeliveryAnalyticsActivated(object sender, EventArgs e)
	{
		var districtSelectorFactory = new EntityAutocompleteSelectorFactory<DistrictJournalViewModel>(typeof(District), () =>
		{
			var filter = new DistrictJournalFilterViewModel { Status = DistrictsSetStatus.Active };
			return new DistrictJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices)
			{
				EnableDeleteButton = true,
				EnableAddButton = true,
				EnableEditButton = true
			};
		});

		tdiMain.AddTab(
			new DeliveryAnalyticsViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.InteractiveService,
				NavigationManager,
				districtSelectorFactory)
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
		var entityChangeWatcher = NotifyConfiguration.Instance;
		var uowFactory = autofacScope.Resolve<IUnitOfWorkFactory>();
		var interactiveService = autofacScope.Resolve<IInteractiveService>();
		var carSelectorFactory = new CarJournalFactory(NavigationManager);
		IFileDialogService fileDialogService = new FileDialogService();

		var viewModel = new CostCarExploitationReportViewModel(
			uowFactory, 
			interactiveService, 
			NavigationManager, 
			carSelectorFactory, 
			entityChangeWatcher, 
			fileDialogService);

		tdiMain.AddTab(viewModel);
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
}
