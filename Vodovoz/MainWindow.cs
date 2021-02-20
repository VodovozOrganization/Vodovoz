using System;
using System.Linq;
using Autofac;
using Gtk;
using NLog;
using QS.Banks.Domain;
using QS.BusinessCommon.Domain;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Project.Services.Interactive;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Tools;
using QSBanks;
using QSOrmProject;
using QSProjectsLib;
using QSSupportLib;
using Vodovoz;
using Vodovoz.Core;
using Vodovoz.Dialogs.OnlineStore;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Service.BaseParametersServices;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.StoredResources;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Cash.Requests;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Journals.JournalViewModels.WageCalculation;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewModels;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bookkeeping;
using Vodovoz.ReportsParameters.Bottles;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.ReportsParameters.Payments;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.ReportsParameters.Store;
using Vodovoz.Representations;
using Vodovoz.ServiceDialogs;
using Vodovoz.ServiceDialogs.Database;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Users;
using Vodovoz.ViewWidgets;
using ToolbarStyle = Vodovoz.Domain.Employees.ToolbarStyle;
using Vodovoz.ReportsParameters.Production;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using VodovozInfrastructure.Interfaces;
using Vodovoz.Parameters;
using Vodovoz.Journals;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Proposal;
using Vodovoz.ViewModels.Accounting;
using Vodovoz.Tools.Logistic;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalViewModels.Security;
using Vodovoz.ViewModels.Journals.FilterViewModels.Security;

public partial class MainWindow : Gtk.Window
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
    uint LastUiId;

    public TdiNotebook TdiMain => tdiMain;

    private ILifetimeScope AutofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
    public TdiNavigationManager NavigationManager;
    public MangoManager MangoManager;

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();
        PerformanceHelper.AddTimePoint("Закончена стандартная сборка окна.");
        this.BuildToolbarActions();
        tdiMain.WidgetResolver = ViewModelWidgetResolver.Instance;
        TDIMain.MainNotebook = tdiMain;
        this.KeyReleaseEvent += TDIMain.TDIHandleKeyReleaseEvent;
        this.Title = MainSupport.GetTitle();
        //Настраиваем модули
        ActionUsers.Sensitive = QSMain.User.Admin;
        ActionAdministration.Sensitive = QSMain.User.Admin;
        labelUser.LabelProp = QSMain.User.Name;
        ActionCash.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("role_сashier");
        ActionAccounting.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("money_manage_bookkeeping");
        ActionRouteListsAtDay.Sensitive =
            ActionRouteListTracking.Sensitive =
            ActionRouteListMileageCheck.Sensitive =
            ActionRouteListAddressesTransferring.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("logistican");
        ActionStock.Sensitive = CurrentPermissions.Warehouse.Allowed().Any();

        bool hasAccessToCRM = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("access_to_crm");
        bool hasAccessToSalaries = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("access_to_salaries");
        bool hasAccessToWagesAndBonuses = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("access_to_fines_bonuses");
        ActionEmployeesBonuses.Sensitive = hasAccessToWagesAndBonuses;
        ActionEmployeeFines.Sensitive = hasAccessToWagesAndBonuses;
        ActionDriverWages.Sensitive = hasAccessToSalaries;
        ActionWagesOperations.Sensitive = hasAccessToSalaries;
        ActionForwarderWageReport.Sensitive = hasAccessToSalaries;
        ActionDriversWageBalance.Visible = hasAccessToSalaries;
        ActionCRM.Sensitive = hasAccessToCRM;

        ActionWage.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_wage");

        ActionFinesJournal.Visible = ActionPremiumJournal.Visible = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("access_to_fines_bonuses");
        ActionReports.Sensitive = false;
        //ActionServices.Visible = false;
        ActionDocTemplates.Visible = QSMain.User.Admin;
        ActionService.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("database_maintenance");
        ActionEmployeeWorkChart.Sensitive = false;

        ActionAddOrder.Sensitive = ServicesConfig.CommonServices.PermissionService.ValidateUserPermission(typeof(Order), QSMain.User.Id)?.CanCreate ?? false;
        ActionExportImportNomenclatureCatalog.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_and_arc_nomenclatures");
        ActionDistricts.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DistrictsSet)).CanRead;

        //Читаем настройки пользователя
        switch (CurrentUserSettings.Settings.ToolbarStyle)
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

        switch (CurrentUserSettings.Settings.ToolBarIconsSize)
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

        NavigationManager = AutofacScope.Resolve<TdiNavigationManager>(new TypedParameter(typeof(TdiNotebook), tdiMain));
        MangoManager = AutofacScope.Resolve<MangoManager>(new TypedParameter(typeof(Gtk.Action), MangoAction));
        MangoManager.Connect();

        BanksUpdater.CheckBanksUpdate(false);

        // Блокировка отчетов для торговых представителей

        bool userIsSalesRepresentative;

        using (var uow = UnitOfWorkFactory.CreateWithoutRoot()){
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
            Action61.Visible = // Касса
            Action70.Visible = !userIsSalesRepresentative; // Производство

        // Отчеты в Продажи

        ActionOrderCreationDateReport.Visible = 
            ActionPlanImplementationReport.Visible =
            ActionSetBillsReport.Visible = !userIsSalesRepresentative;

        // Управление ограничением доступа через зарегистрированные RM

        var userCanManageRegisteredRMs = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("user_can_manage_registered_rms");

        registeredRMAction.Visible = userCanManageRegisteredRMs;
    }

    public void OnTdiMainTabAdded(object sender, TabAddedEventArgs args)
    {
        if (args.Tab is IInfoProvider dialogTab)
            dialogTab.CurrentObjectChanged += infopanel.OnCurrentObjectChanged;
        else if (args.Tab is TdiSliderTab journalTab && journalTab.Journal is IInfoProvider journal)
            journal.CurrentObjectChanged += infopanel.OnCurrentObjectChanged;
    }

    public void OnTdiMainTabClosed(object sender, TabClosedEventArgs args)
    {
        if (args.Tab is IInfoProvider dialogTab)
            infopanel.OnInfoProviderDisposed(dialogTab);
        else if (args.Tab is TdiSliderTab journalTab && journalTab.Journal is IInfoProvider journal)
            infopanel.OnInfoProviderDisposed(journal);
        if (tdiMain.NPages == 0)
            infopanel.SetInfoProvider(DefaultInfoProvider.Instance);
    }

    public void OnTdiMainTabSwitched(object sender, TabSwitchedEventArgs args)
    {
        var currentTab = args.Tab;
        if (currentTab is IInfoProvider)
            infopanel.SetInfoProvider(currentTab as IInfoProvider);
        else if (currentTab is TdiSliderTab && (currentTab as TdiSliderTab).Journal is IInfoProvider)
            infopanel.SetInfoProvider((currentTab as TdiSliderTab).Journal as IInfoProvider);
        else
            infopanel.SetInfoProvider(DefaultInfoProvider.Instance);
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        if (tdiMain.CloseAllTabs())
        {
            a.RetVal = false;
            AutofacScope.Dispose();
            Application.Quit();
        }
        else
        {
            a.RetVal = true;
        }
    }

    protected void OnQuitActionActivated(object sender, EventArgs e)
    {
        if (tdiMain.CloseAllTabs())
        {
            AutofacScope.Dispose();
            Application.Quit();
        }
    }

    protected void OnDialogAuthenticationActionActivated(object sender, EventArgs e)
    {
        QSMain.User.ChangeUserPassword(this);
    }

    protected void OnAboutActionActivated(object sender, EventArgs e)
    {
        QSMain.RunAboutDialog();
    }

    protected void OnActionOrdersToggled(object sender, EventArgs e)
    {
        if (ActionOrders.Active)
            SwitchToUI("Vodovoz.toolbars.orders.xml");
    }

    private void SwitchToUI(string uiResource)
    {
        if (LastUiId > 0)
        {
            this.UIManager.RemoveUi(LastUiId);
            LastUiId = 0;
        }
        LastUiId = this.UIManager.AddUiFromResource(uiResource);
        this.UIManager.EnsureUpdate();
    }

    protected void OnActionServicesToggled(object sender, EventArgs e)
    {
        if (ActionServices.Active)
            SwitchToUI("Vodovoz.toolbars.services.xml");
    }

    protected void OnActionLogisticsToggled(object sender, EventArgs e)
    {
        if (ActionLogistics.Active)
            SwitchToUI("logistics.xml");
    }

    protected void OnActionStockToggled(object sender, EventArgs e)
    {
        if (ActionStock.Active)
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
        var employeesJournal = new EmployeesJournalViewModel(employeeFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
        tdiMain.AddTab(employeesJournal);
    }

    protected void OnActionCarsActivated(object sender, EventArgs e)
    {
        CarJournalFilterViewModel filter = new CarJournalFilterViewModel();
        var carJournal = new CarJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
        tdiMain.AddTab(carJournal);
    }

    protected void OnActionUnitsActivated(object sender, EventArgs e)
    {
        OrmReference refWin = new OrmReference(typeof(MeasurementUnits));
        tdiMain.AddTab(refWin);
    }

    protected void OnActionDiscountReasonsActivated(object sender, EventArgs e)
    {
        OrmReference refWin = new OrmReference(typeof(DiscountReason));
        tdiMain.AddTab(refWin);
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

    protected void OnActionEquipmentTypesActivated(object sender, EventArgs e)
    {
        OrmReference refWin = new OrmReference(typeof(EquipmentType));
        tdiMain.AddTab(refWin);
    }

    protected void OnActionNomenclatureActivated(object sender, EventArgs e)
    {
        var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());

        IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
            new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
                CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);

        IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
            new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig
                .CommonServices, new NomenclatureFilterViewModel(), counterpartySelectorFactory,
                nomenclatureRepository, UserSingletonRepository.GetInstance());

        tdiMain.OpenTab(
            () =>
            {
                return new NomenclaturesJournalViewModel(
                    new NomenclatureFilterViewModel() { HidenByDefault = true },
                    UnitOfWorkFactory.GetDefaultFactory,
                    ServicesConfig.CommonServices,
                    VodovozGtkServicesConfig.EmployeeService,
                    nomenclatureSelectorFactory,
                    counterpartySelectorFactory,
                    nomenclatureRepository,
                    UserSingletonRepository.GetInstance()
                );
            }
        );
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
        CounterpartyJournalFilterViewModel filter = new CounterpartyJournalFilterViewModel();
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

    protected void OnActionPaidRentPackageActivated(object sender, EventArgs e)
    {
        OrmReference refWin = new OrmReference(typeof(PaidRentPackage));
        tdiMain.AddTab(refWin);
    }

    protected void OnActionEquipmentActivated(object sender, EventArgs e)
    {
        OrmReference refWin = new OrmReference(typeof(Equipment));
        tdiMain.AddTab(refWin);
    }

    protected void OnActionDeliveryScheduleActivated(object sender, EventArgs e)
    {
        OrmReference refWin = new OrmReference(typeof(DeliverySchedule));
        tdiMain.AddTab(refWin);
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
        tdiMain.OpenTab(
            OrmReference.GenerateHashName<FuelType>(),
            () => new OrmReference(typeof(FuelType))
        );
    }

    protected void OnActionDeliveryShiftActivated(object sender, EventArgs e)
    {
        OrmReference refWin = new OrmReference(typeof(DeliveryShift));
        tdiMain.AddTab(refWin);
    }

    protected void OnActionParametersActivated(object sender, EventArgs e)
    {
        var config = new ApplicationConfigDialog();
        config.ShowAll();
        config.Run();
        config.Destroy();
    }

    protected void OnAction14Activated(object sender, EventArgs e)
    {
        var incomeCategoryFilter = new IncomeCategoryJournalFilterViewModel();
        IFileChooserProvider chooserProvider = new Vodovoz.FileChooser("Категории прихода.csv");

        tdiMain.AddTab(
            new IncomeCategoryJournalViewModel(
                incomeCategoryFilter,
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices,
                chooserProvider
            )
        );
    }

    protected void OnAction15Activated(object sender, EventArgs e)
    {
        var expenseCategoryFilter = new ExpenseCategoryJournalFilterViewModel();
        IFileChooserProvider chooserProvider = new Vodovoz.FileChooser("Категории расхода.csv");

        tdiMain.AddTab(
            new ExpenseCategoryJournalViewModel(
                expenseCategoryFilter,
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices,
                chooserProvider
            )
        );
    }

    protected void OnActionCashToggled(object sender, EventArgs e)
    {
        if (ActionCash.Active)
            SwitchToUI("cash.xml");
    }

    protected void OnActionAccountingToggled(object sender, EventArgs e)
    {
        if (ActionAccounting.Active)
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
        if (ActionToolBarText.Active)
            ToolBarMode(ToolbarStyle.Text);
    }

    private void ToolBarMode(ToolbarStyle style)
    {
        if (CurrentUserSettings.Settings.ToolbarStyle != style)
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
        if (CurrentUserSettings.Settings.ToolBarIconsSize != size)
        {
            CurrentUserSettings.Settings.ToolBarIconsSize = size;
            CurrentUserSettings.SaveSettings();
        }
        switch (size)
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
        if (ActionToolBarIcon.Active)
            ToolBarMode(ToolbarStyle.Icons);
    }

    protected void OnActionToolBarBothToggled(object sender, EventArgs e)
    {
        if (ActionToolBarBoth.Active)
            ToolBarMode(ToolbarStyle.Both);
    }

    protected void OnActionIconsExtraSmallToggled(object sender, EventArgs e)
    {
        if (ActionIconsExtraSmall.Active)
            ToolBarMode(IconsSize.ExtraSmall);
    }

    protected void OnActionIconsSmallToggled(object sender, EventArgs e)
    {
        if (ActionIconsSmall.Active)
            ToolBarMode(IconsSize.Small);
    }

    protected void OnActionIconsMiddleToggled(object sender, EventArgs e)
    {
        if (ActionIconsMiddle.Active)
            ToolBarMode(IconsSize.Middle);
    }

    protected void OnActionIconsLargeToggled(object sender, EventArgs e)
    {
        if (ActionIconsLarge.Active)
            ToolBarMode(IconsSize.Large);
    }

    protected void OnActionDeliveryPointsActivated(object sender, EventArgs e)
    {
        DeliveryPointJournalFilterViewModel filter = new DeliveryPointJournalFilterViewModel();
        var deliveryPointJournal = new DeliveryPointJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
        tdiMain.AddTab(deliveryPointJournal);
    }

    protected void OnPropertiesActionActivated(object sender, EventArgs e)
    {
        tdiMain.OpenTab(
            () =>
            {
                return new UserSettingsViewModel(
                    EntityUoWBuilder.ForOpen(CurrentUserSettings.Settings.Id),
                    UnitOfWorkFactory.GetDefaultFactory,
                    ServicesConfig.CommonServices
                );
            }
        );
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
        if (ActionArchive.Active)
            SwitchToUI("archive.xml");
    }

    protected void OnActionStaffToggled(object sender, EventArgs e)
    {
        if (ActionStaff.Active)
            SwitchToUI("Vodovoz.toolbars.staff.xml");
    }

    protected void OnActionComplaintsActivated(object sender, EventArgs e)
    {
        IUndeliveriesViewOpener undeliveriesViewOpener = new UndeliveriesViewOpener();

        var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());

        IEntityAutocompleteSelectorFactory employeeSelectorFactory =
            new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(
                ServicesConfig.CommonServices);

        IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
            new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
                CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);

        IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
            new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig
                .CommonServices, new NomenclatureFilterViewModel(), counterpartySelectorFactory,
                nomenclatureRepository, UserSingletonRepository.GetInstance());

        ISubdivisionRepository subdivisionRepository = new SubdivisionRepository();
        IRouteListItemRepository routeListItemRepository = new RouteListItemRepository();
        IFilePickerService filePickerService = new GtkFilePicker();

        tdiMain.OpenTab(
            () =>
            {
                return new ComplaintsJournalViewModel(
                    UnitOfWorkFactory.GetDefaultFactory,
                    ServicesConfig.CommonServices,
                    undeliveriesViewOpener,
                    VodovozGtkServicesConfig.EmployeeService,
                    employeeSelectorFactory,
                    counterpartySelectorFactory,
                    nomenclatureSelectorFactory,
                    routeListItemRepository,
                    SubdivisionParametersProvider.Instance,
                    new ComplaintFilterViewModel(
                        ServicesConfig.CommonServices,
                        subdivisionRepository,
                        employeeSelectorFactory
                    ),
                    filePickerService,
                    subdivisionRepository,
                    new GtkReportViewOpener(),
                    new GtkTabsOpener(),
                    nomenclatureRepository,
                    UserSingletonRepository.GetInstance()
                );
            }
        );
    }

    protected void OnActionSalesReportActivated(object sender, EventArgs e)
    {
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.SalesReport>(),
            () => new QSReport.ReportViewDlg(new Vodovoz.Reports.SalesReport(EmployeeSingletonRepository.GetInstance()))
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
            () => new QSReport.ReportViewDlg(new Vodovoz.Reports.EquipmentReport())
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
            QSReport.ReportViewDlg.GenerateHashName<Vodovoz.Reports.DriversWageBalanceReport>(),
            () => new QSReport.ReportViewDlg(new Vodovoz.Reports.DriversWageBalanceReport())
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

    protected void OnActionRoutesListRegisterActivated(object sender, EventArgs e) => OpenDriverRoutesListRegisterReport();
    protected void OnActionOrderedByIdRoutesListRegisterActivated(object sender, EventArgs e) => OpenRoutesListRegisterReport();
    protected void OnActionProducedProductionReportActivated(object sender, EventArgs e)
    {
        #region DependencyCreation
        var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());

        IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
            new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
                CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);

        IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
            new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
                new NomenclatureFilterViewModel(), counterpartySelectorFactory, nomenclatureRepository,
                UserSingletonRepository.GetInstance());

        #endregion

        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<ProducedProductionReport>(),
            () => new QSReport.ReportViewDlg(new ProducedProductionReport(counterpartySelectorFactory, nomenclatureSelectorFactory, nomenclatureRepository))
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
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.DeliveryTimeReport>(),
            () => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.DeliveryTimeReport())
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
        tdiMain.OpenTab(
            TdiTabBase.GenerateHashName<MergeAddressesDlg>(),
            () => new MergeAddressesDlg()
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
            QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Bottles.BottlesMovementReport>(),
            () => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Bottles.BottlesMovementReport())
        );
    }

    protected void OnActionMileageReportActivated(object sender, EventArgs e)
    {
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<Vodovoz.ReportsParameters.Logistic.MileageReport>(),
            () => new QSReport.ReportViewDlg(new Vodovoz.ReportsParameters.Logistic.MileageReport())
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
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<MastersVisitReport>(),
            () => new QSReport.ReportViewDlg(new MastersVisitReport())
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
        OrmReference refWin = new OrmReference(typeof(PremiumTemplate));
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
            () => new QSReport.ReportViewDlg(new CardPaymentsOrdersReport())
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

    protected void OnActionSendedBillsActivated(object sender, EventArgs e)
    {
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<SendedEmailsReport>(),
            () => new QSReport.ReportViewDlg(new SendedEmailsReport())
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
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<PaymentsFromTinkoffReport>(),
            () => new QSReport.ReportViewDlg(new PaymentsFromTinkoffReport())
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
        if (QSMain.User.Admin)
            tdiMain.OpenTab(
                OrmReference.GenerateHashName<TypeOfEntity>(),
                () => new OrmReference(typeof(TypeOfEntity))
            );
    }

    protected void OnActionUsersActivated(object sender, EventArgs e)
    {
        UsersDialog usersDlg = new UsersDialog();
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
            OrmReference.GenerateHashName<StoredImageResource>(),
            () => new OrmReference(typeof(StoredImageResource))
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
            () => new QSReport.ReportViewDlg(new FirstClientsReport())
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
        tdiMain.OpenTab(
            OrmReference.GenerateHashName<TariffZone>(),
            () => new OrmReference(typeof(TariffZone))
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
        var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());

        IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
            new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
                CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);

        IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory =
            new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig
                .CommonServices, new NomenclatureFilterViewModel(), counterpartySelectorFactory, nomenclatureRepository,
                UserSingletonRepository.GetInstance());

        tdiMain.AddTab(
            new PromotionalSetsJournalViewModel(
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices,
                VodovozGtkServicesConfig.EmployeeService,
                counterpartySelectorFactory,
                nomenclatureSelectorFactory,
                nomenclatureRepository,
                UserSingletonRepository.GetInstance()
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
            () => new QSReport.ReportViewDlg(new FirstSecondClientReport())
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
            () => new QSReport.ReportViewDlg(new CashBookReport(new SubdivisionRepository(), ServicesConfig.CommonServices))
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
        tdiMain.OpenTab(
            OrmReference.GenerateHashName<PaymentFrom>(),
            () => new OrmReference(typeof(PaymentFrom))
        );
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

    protected void OnActionComplaintResultActivated(object sender, EventArgs e)
    {
        var complaintResultsViewModel = new SimpleEntityJournalViewModel<ComplaintResult, ComplaintResultViewModel>(
            x => x.Name,
            () => new ComplaintResultViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices),
            (node) => new ComplaintResultViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices),
            UnitOfWorkFactory.GetDefaultFactory,
            ServicesConfig.CommonServices
        );
        tdiMain.AddTab(complaintResultsViewModel);
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
                ServicesConfig.CommonServices
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
        var complaintKindsViewModel = new SimpleEntityJournalViewModel<ComplaintKind, ComplaintKindViewModel>(
            x => x.Name,
            () => new ComplaintKindViewModel(
                EntityUoWBuilder.ForCreate(),
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices
            ),
            (node) => new ComplaintKindViewModel(
                EntityUoWBuilder.ForOpen(node.Id),
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices
            ),
            UnitOfWorkFactory.GetDefaultFactory,
            ServicesConfig.CommonServices
        );
        complaintKindsViewModel.SetActionsVisible(deleteActionEnabled: false);
        tdiMain.AddTab(complaintKindsViewModel);
    }

    protected void OnActionSetBillsReportActivated(object sender, EventArgs e)
    {
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<SetBillsReport>(),
            () => new QSReport.ReportViewDlg(new SetBillsReport(UnitOfWorkFactory.GetDefaultFactory))
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
            () => new QSReport.ReportViewDlg(new WayBillReport())
        );
    }

    protected void OnActionPaymentsReportActivated(object sender, EventArgs e)
    {
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<PaymentsFromBankClientReport>(),
            () => new QSReport.ReportViewDlg(new PaymentsFromBankClientReport())
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
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<ChainStoreDelayReport>(),
            () => new QSReport.ReportViewDlg(new ChainStoreDelayReport())
        );
    }

    protected void OnActionReturnedTareReportActivated(object sender, EventArgs e)
    {
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<ReturnedTareReport>(),
            () => new QSReport.ReportViewDlg(new ReturnedTareReport())
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
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<ProductionRequestReport>(),
            () => new QSReport.ReportViewDlg(new ProductionRequestReport())
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
        var cashRequestFilterViewModel = new CashRequestJournalFilterViewModel();
        IFileChooserProvider chooserProvider = new Vodovoz.FileChooser("Категории расхода.csv");

        ISubdivisionRepository subdivisionRepository = new SubdivisionRepository();
        ICashRequestRepository cashRequestRepository = new CashRequestRepository();
        IEmployeeRepository employeeRepository = EmployeeSingletonRepository.GetInstance();
        CashRepository cashRepository = new CashRepository();
        ConsoleInteractiveService consoleInteractiveService = new ConsoleInteractiveService();
        tdiMain.AddTab(
            new CashRequestJournalViewModel(
                cashRequestFilterViewModel,
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices,
                chooserProvider,
                employeeRepository,
                cashRepository,
                consoleInteractiveService
            )
        );
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
        var employeesAutocompleteSelectionFactory = new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
            () =>
            {
                var employeeFilter = new EmployeeFilterViewModel
                {
                    Status = EmployeeStatus.IsWorking,
                };
                return new EmployeesJournalViewModel(
                    employeeFilter,
                    UnitOfWorkFactory.GetDefaultFactory,
                    ServicesConfig.CommonServices);
            });

        tdiMain.OpenTab(
            () =>
            {
                return new WayBillGeneratorViewModel
                (
                    UnitOfWorkFactory.GetDefaultFactory,
                    ServicesConfig.CommonServices.InteractiveService,
                    NavigationManagerProvider.NavigationManager,
                    new WayBillDocumentRepository(),
                    new RouteGeometryCalculator(DistanceProvider.Osrm),
                    employeesAutocompleteSelectionFactory
                );
            }
        );
    }

    protected void OnActionOrderChangesReportActivated(object sender, EventArgs e)
    {
        tdiMain.OpenTab(
            QSReport.ReportViewDlg.GenerateHashName<OrderChangesReport>(),
            () => new QSReport.ReportViewDlg(new OrderChangesReport())
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
        ) ;
    }
}
