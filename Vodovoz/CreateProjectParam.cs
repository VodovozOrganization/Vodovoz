using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Autofac;
using QS.Deletion;
using QS.Deletion.Configuration;
using QS.Deletion.ViewModels;
using QS.Deletion.Views;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Services;
using QS.Project.Services.GtkUI;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Extension;
using QS.ViewModels.Resolve;
using QS.Views.Resolve;
using QS.Widgets.GtkUI;
using QSProjectsLib;
using QSReport;
using Vodovoz.Core;
using Vodovoz.Core.Permissions;
using Vodovoz.Dialogs.Cash;
using Vodovoz.Dialogs.Client;
using Vodovoz.Dialogs.Email;
using Vodovoz.Dialogs.Fuel;
using Vodovoz.Domain.Store;
using Vodovoz.Filters.GtkViews;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.Footers.ViewModels;
using Vodovoz.Footers.Views;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.Infrastructure.Permissions;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalColumnsConfigs;
using Vodovoz.Journals.FilterViewModels;
using Vodovoz.JournalViewers;
using Vodovoz.PermissionExtensions;
using Vodovoz.ReportsParameters;
using Vodovoz.Services.Permissions;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.BusinessTasks;
using Vodovoz.ViewModels.Cash;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs.Fuel;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewModels.Orders.OrdersWithoutShipment;
using Vodovoz.ViewModels.ViewModels.Organizations;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.Reports;
using Vodovoz.ViewModels.Suppliers;
using Vodovoz.ViewModels.Users;
using Vodovoz.ViewModels.WageCalculation;
using Vodovoz.ViewModels.WageCalculation.AdvancedWageParameterViewModels;
using Vodovoz.ViewModels.Warehouses;
using Vodovoz.Views;
using Vodovoz.Views.BusinessTasks;
using Vodovoz.Views.Cash;
using Vodovoz.Views.Complaints;
using Vodovoz.Views.Contacts;
using Vodovoz.Views.Employees;
using Vodovoz.Views.Logistic;
using Vodovoz.Views.Orders;
using Vodovoz.Views.Orders.OrdersWithoutShipment;
using Vodovoz.Views.Organization;
using Vodovoz.Views.Suppliers;
using Vodovoz.Views.Users;
using Vodovoz.Views.WageCalculation;
using Vodovoz.Views.Warehouse;
using Vodovoz.ViewWidgets.AdvancedWageParameterViews;
using Vodovoz.ViewWidgets.Permissions;
using Vodovoz.ViewWidgets.PromoSetAction;
using Vodovoz.ViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels;
using Vodovoz.ViewModels.ViewModels.Cash;
using Vodovoz.Views.Goods;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.JournalFilters;
using Vodovoz.Views.Mango.Talks;
using Vodovoz.ViewModels.Mango.Talks;
using Vodovoz.ViewModels.ViewModels;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Store;
using Vodovoz.Views.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;
using Vodovoz.JournalFilters.Proposal;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Proposal;
using Vodovoz.Views.Proposal;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Security;
using Vodovoz.Views.Security;
using Vodovoz.ViewModels.Journals.FilterViewModels.Security;
using Vodovoz.ViewModels.Journals.FilterViewModels.Retail;
using Vodovoz.ViewModels.ViewModels.Retail;
using Vodovoz.Views.Retail;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Complaints;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.Views.Reports;
using Vodovoz.ViewModels.ViewModels.Orders;
using Vodovoz.ViewModels.ViewModels.Reports;
using Vodovoz.JournalViewers.Complaints;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModels.Dialogs.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Cash;
using Vodovoz.Views.Client;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using Vodovoz.ViewModels.ViewModels.Flyers;
using Vodovoz.ViewModels.ViewModels.Rent;
using Vodovoz.ViewModels.ViewModels.Suppliers;
using Vodovoz.Views.Flyers;
using Vodovoz.Views.Print;
using Vodovoz.Views.Rent;
using ProductGroupView = Vodovoz.Views.Goods.ProductGroupView;

namespace Vodovoz
{
	partial class MainClass
	{
		static void CreateProjectParam()
		{
			UserDialog.RequestWidth = 900;
			UserDialog.RequestHeight = 700;

			UserDialog.UserPermissionViewsCreator = () => new List<IUserPermissionTab> {
				new SubdivisionForUserEntityPermissionWidget(),
				new PresetPermissionsView()
			};

			UserDialog.PermissionViewsCreator = () => new List<IPermissionsView>
			{
				new PermissionMatrixView(
					new PermissionMatrix<WarehousePermissions, Warehouse>(), "Доступ к складам", "warehouse_access")
			};

			WarehousePermissionService.WarehousePermissionValidatorFactory = new WarehousePermissionValidatorFactory();
		}

		static void ConfigureViewModelWidgetResolver()
		{
			ViewModelWidgetResolver.Instance = new BasedOnNameViewModelWidgetResolver();

			//Регистрация вкладок
			ViewModelWidgetResolver.Instance
				.RegisterWidgetForTabViewModel<DistrictViewModel, DistrictView>()
				.RegisterWidgetForTabViewModel<DistrictsSetActivationViewModel, DistrictsSetActivationView>()
				.RegisterWidgetForTabViewModel<FuelTransferDocumentViewModel, FuelTransferDocumentView>()
				.RegisterWidgetForTabViewModel<FuelIncomeInvoiceViewModel, FuelIncomeInvoiceView>()
				.RegisterWidgetForTabViewModel<ClientCameFromViewModel, ClientCameFromView>()
				.RegisterWidgetForTabViewModel<FuelTypeViewModel, FuelTypeView>()
				.RegisterWidgetForTabViewModel<FuelWriteoffDocumentViewModel, FuelWriteoffDocumentView>()
				.RegisterWidgetForTabViewModel<ResidueViewModel, ResidueView>()
				.RegisterWidgetForTabViewModel<FineTemplateViewModel, FineTemplateView>()
				.RegisterWidgetForTabViewModel<ComplaintViewModel, ComplaintView>()
				.RegisterWidgetForTabViewModel<IncomeCategoryViewModel, IncomeCategoryView>()
				.RegisterWidgetForTabViewModel<ExpenseCategoryViewModel, ExpenseCategoryView>()
				.RegisterWidgetForTabViewModel<CashRequestViewModel, CashRequestView>()
				.RegisterWidgetForTabViewModel<CashRequestItemViewModel, CashRequestItemView>()
				.RegisterWidgetForTabViewModel<CreateComplaintViewModel, CreateComplaintView>()
				.RegisterWidgetForTabViewModel<CreateInnerComplaintViewModel, CreateInnerComplaintView>()
				.RegisterWidgetForTabViewModel<ComplaintSourceViewModel, ComplaintSourceView>()
				.RegisterWidgetForTabViewModel<ComplaintResultViewModel, ComplaintResultView>()
				.RegisterWidgetForTabViewModel<SubdivisionViewModel, SubdivisionView>()
				.RegisterWidgetForTabViewModel<FineViewModel, FineView>()
				.RegisterWidgetForTabViewModel<RequestToSupplierViewModel, RequestToSupplierView>()
				.RegisterWidgetForTabViewModel<WageDistrictViewModel, WageDistrictView>()
				.RegisterWidgetForTabViewModel<WageDistrictLevelRatesViewModel, WageDistrictLevelRatesView>()
				.RegisterWidgetForTabViewModel<EmployeeWageParameterViewModel, WageParameterView>()
				.RegisterWidgetForTabViewModel<SalesPlanViewModel, SalesPlanView>()
				.RegisterWidgetForTabViewModel<RouteListsOnDayViewModel, RouteListsOnDayView>()
				.RegisterWidgetForTabViewModel<FuelDocumentViewModel, FuelDocumentView>()
				.RegisterWidgetForTabViewModel<DriverWorkScheduleSetViewModel, DriverWorkScheduleSetView>()
				.RegisterWidgetForTabViewModel<DriverDistrictPrioritySetViewModel, DriverDistrictPrioritySetView>()
				.RegisterWidgetForTabViewModel<ComplaintKindViewModel, ComplaintKindView>()
				.RegisterWidgetForTabViewModel<MovementDocumentViewModel, MovementDocumentView>()
				.RegisterWidgetForTabViewModel<IncomingInvoiceViewModel, IncomingInvoiceView>()
				.RegisterWidgetForTabViewModel<PhoneTypeViewModel, PhoneTypeView>()
				.RegisterWidgetForTabViewModel<EmailTypeViewModel, EmailTypeView>()
				.RegisterWidgetForTabViewModel<UserSettingsViewModel, UserSettingsView>()
				.RegisterWidgetForTabViewModel<PaymentLoaderViewModel, PaymentLoaderView>()
				.RegisterWidgetForTabViewModel<ManualPaymentMatchingViewModel, ManualPaymentMatchingView>()
				.RegisterWidgetForTabViewModel<ClientTaskViewModel, ClientTaskView>()
				.RegisterWidgetForTabViewModel<DriverCarKindViewModel, DriverCarKindView>()
				.RegisterWidgetForTabViewModel<PaymentTaskViewModel, PaymentTaskView>()
				.RegisterWidgetForTabViewModel<DistrictsSetViewModel, DistrictsSetView>()
				.RegisterWidgetForTabViewModel<AcceptBeforeViewModel, AcceptBeforeView>()
				.RegisterWidgetForTabViewModel<OrderWithoutShipmentForDebtViewModel, OrderWithoutShipmentForDebtView>()
				.RegisterWidgetForTabViewModel<OrderWithoutShipmentForPaymentViewModel, OrderWithoutShipmentForPaymentView>()
				.RegisterWidgetForTabViewModel<OrderWithoutShipmentForAdvancePaymentViewModel, OrderWithoutShipmentForAdvancePaymentView>()
				.RegisterWidgetForTabViewModel<ReturnTareReasonCategoryViewModel, ReturnTareReasonCategoryView>()
				.RegisterWidgetForTabViewModel<ReturnTareReasonViewModel, ReturnTareReasonView>()
				.RegisterWidgetForTabViewModel<PaymentByCardViewModel, PaymentByCardView>()
				.RegisterWidgetForTabViewModel<RouteListAnalysisViewModel, RouteListAnalysisView>()
				.RegisterWidgetForTabViewModel<DriversInfoExportViewModel, DriversInfoExportView>()
				.RegisterWidgetForTabViewModel<LateArrivalReasonViewModel, LateArrivalReasonView>()
				.RegisterWidgetForTabViewModel<NomenclatureViewModel, NomenclatureView>()
				.RegisterWidgetForTabViewModel<FinancialDistrictsSetViewModel, FinancialDistrictsSetView>()
				.RegisterWidgetForTabViewModel<MovementWagonViewModel, MovementWagonView>()
				.RegisterWidgetForTabViewModel<UserViewModel, UserView>()
                .RegisterWidgetForTabViewModel<ApplicationDevelopmentProposalViewModel, ApplicationDevelopmentProposalView>()
				.RegisterWidgetForTabViewModel<RegisteredRMViewModel, RegisteredRMView>()
                .RegisterWidgetForTabViewModel<SalesChannelViewModel, SalesChannelView>()
                .RegisterWidgetForTabViewModel<DeliveryPointResponsiblePersonTypeViewModel, DeliveryPointResponsiblePersonTypeView>()
                .RegisterWidgetForTabViewModel<NomenclaturePlanViewModel, NomenclaturePlanView>()
                .RegisterWidgetForTabViewModel<NomenclaturePlanReportViewModel, NomenclaturePlanReportView>()
                .RegisterWidgetForTabViewModel<OrganizationCashTransferDocumentViewModel, OrganizationCashTransferDocumentView>()
				.RegisterWidgetForTabViewModel<PremiumViewModel, PremiumView>()
				.RegisterWidgetForTabViewModel<PremiumRaskatGAZelleViewModel, PremiumRaskatGAZelleView>()
				.RegisterWidgetForTabViewModel<PremiumTemplateViewModel, PremiumTemplateView>()
				.RegisterWidgetForTabViewModel<CarEventTypeViewModel, CarEventTypeView>()
				.RegisterWidgetForTabViewModel<CarEventViewModel, CarEventView>()
				.RegisterWidgetForTabViewModel<DiscountReasonViewModel, DiscountReasonView>()
				.RegisterWidgetForTabViewModel<EmployeeViewModel, EmployeeView>()
				.RegisterWidgetForTabViewModel<DriverComplaintReasonViewModel, DriverComplaintReasonView>()
				.RegisterWidgetForTabViewModel<FlyerViewModel, FlyerView>()
				.RegisterWidgetForTabViewModel<ComplaintObjectViewModel, ComplaintObjectView>()
				.RegisterWidgetForTabViewModel<DriverAttachedTerminalViewModel, DriverAttachedTerminalView>()
				.RegisterWidgetForTabViewModel<DeliveryPointViewModel, DeliveryPointView>()
				.RegisterWidgetForTabViewModel<DocumentsPrinterViewModel, DocumentsPrinterView>()
				.RegisterWidgetForTabViewModel<EquipmentKindViewModel, EquipmentKindView>()
				.RegisterWidgetForTabViewModel<ProductGroupViewModel, ProductGroupView>()
				.RegisterWidgetForTabViewModel<UndeliveryTransferAbsenceReasonViewModel, UndeliveryTransferAbsenceReasonView>()
				.RegisterWidgetForTabViewModel<NomenclaturePurchasePriceViewModel, NomenclaturePurchasePriceView>()
				.RegisterWidgetForTabViewModel<FreeRentPackageViewModel, FreeRentPackageView>()
				.RegisterWidgetForTabViewModel<PaidRentPackageViewModel, PaidRentPackageView>()
				;

            //Регистрация виджетов
            ViewModelWidgetResolver.Instance
				.RegisterWidgetForWidgetViewModel<DistrictsSetJournalFilterViewModel, DistrictsSetJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<DistrictJournalFilterViewModel, DistrictJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<SelectableParameterReportFilterViewModel, SelectableParameterReportFilterView>()
				.RegisterWidgetForWidgetViewModel<ComplaintFilterViewModel, ComplaintFilterView>()
				.RegisterWidgetForWidgetViewModel<CounterpartyJournalFilterViewModel, CounterpartyFilterView>()
				.RegisterWidgetForWidgetViewModel<DebtorsJournalFilterViewModel, DebtorsFilterView>()
				.RegisterWidgetForWidgetViewModel<EmployeeFilterViewModel, EmployeeFilterView>()
				.RegisterWidgetForWidgetViewModel<EmployeeRepresentationFilterViewModel, EmployeeRepresentationFilterView>()
				.RegisterWidgetForWidgetViewModel<OrderJournalFilterViewModel, OrderFilterView>()
				.RegisterWidgetForWidgetViewModel<ClientCameFromFilterViewModel, ClientCameFromFilterView>()
				.RegisterWidgetForWidgetViewModel<ResidueFilterViewModel, ResidueFilterView>()
				.RegisterWidgetForWidgetViewModel<IncomeCategoryJournalFilterViewModel, IncomeCategoryJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ExpenseCategoryJournalFilterViewModel, ExpenseCategoryJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<CashRequestJournalFilterViewModel, CashRequestJournalFilterView>() 
				.RegisterWidgetForWidgetViewModel<ProductGroupFilterViewModel, ProductGroupFilterView>()
				.RegisterWidgetForWidgetViewModel<FineFilterViewModel, FineFilterView>()
				.RegisterWidgetForWidgetViewModel<SubdivisionFilterViewModel, SubdivisionFilterView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureFilterViewModel, NomenclaturesFilterView>()
				.RegisterWidgetForWidgetViewModel<RequestsToSuppliersFilterViewModel, RequestsToSuppliersFilterView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureStockFilterViewModel, NomenclatureStockFilterView>()
				.RegisterWidgetForWidgetViewModel<OrderForMovDocJournalFilterViewModel, OrderForMovDocFilterView>()
				.RegisterWidgetForWidgetViewModel<DriverCarKindJournalFilterViewModel, DriverCarKindJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<BottlesCountAdvancedWageParameterViewModel, BottlesCountAdvancedWageParameterWidget>()
				.RegisterWidgetForWidgetViewModel<DeliveryTimeAdvancedWageParameterViewModel, DeliveryTimeAdvancedWagePrameterView>()
				.RegisterWidgetForWidgetViewModel<AdvancedWageParametersViewModel, AdvancedWageParametersView>()
				.RegisterWidgetForWidgetViewModel<AddFixPriceActionViewModel, AddFixPriceActionView>()
				.RegisterWidgetForWidgetViewModel<CarJournalFilterViewModel, CarFilterView>()
				.RegisterWidgetForWidgetViewModel<PresetSubdivisionPermissionsViewModel, PresetPermissionsView>()
				.RegisterWidgetForWidgetViewModel<DeliveryPointJournalFilterViewModel, DeliveryPointJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<PaymentsJournalFilterViewModel, PaymentsFilterView>()
				.RegisterWidgetForWidgetViewModel<CallTaskFilterViewModel, CallTaskFilterView>()
				.RegisterWidgetForWidgetViewModel<BusinessTasksJournalFooterViewModel, BusinessTasksJournalFooterView>()
				.RegisterWidgetForWidgetViewModel<BusinessTasksJournalActionsViewModel, BusinessTasksJournalActionsView>()
				.RegisterWidgetForWidgetViewModel<SendDocumentByEmailViewModel, SendDocumentByEmailView>()
				.RegisterWidgetForWidgetViewModel<FinancialDistrictsSetsJournalFilterViewModel, FinancialDistrictsSetsJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<FixedPricesViewModel, FixedPricesView>()
				.RegisterWidgetForWidgetViewModel<MovementWagonJournalFilterViewModel, MovementWagonJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<UserJournalFilterViewModel, UserJournalFilterView>()
                .RegisterWidgetForWidgetViewModel<ApplicationDevelopmentProposalsJournalFilterViewModel, ApplicationDevelopmentProposalsJournalFilterView>()
                .RegisterWidgetForWidgetViewModel<RouteListJournalFilterViewModel, RouteListJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<RegisteredRMJournalFilterViewModel, RegisteredRMJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<DeliveryPointResponsiblePersonTypeJournalFilterViewModel, DeliveryPointResponsiblePersonTypeJournalFilterView>()
                .RegisterWidgetForWidgetViewModel<SalesChannelJournalFilterViewModel, SalesChannelJournalFilterView>()
                .RegisterWidgetForWidgetViewModel<EmployeePostViewModel, EmployeePostView>()
                .RegisterWidgetForWidgetViewModel<WarehouseViewModel, WarehouseView>()
				.RegisterWidgetForWidgetViewModel<OrderAnalyticsReportViewModel, OrderAnalyticsReportView>()
				.RegisterWidgetForWidgetViewModel<CarsExploitationReportViewModel, CarsExploitationReportView>()
				.RegisterWidgetForWidgetViewModel<NomenclaturePlanFilterViewModel, NomenclaturePlanFilterView>()
                .RegisterWidgetForWidgetViewModel<OrganizationCashTransferDocumentFilterViewModel, OrganizationCashTransferDocumentFilterView>()
				.RegisterWidgetForWidgetViewModel<PremiumJournalFilterViewModel, PremiumJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<DeliveryAnalyticsViewModel, DeliveryAnalyticsReportView>()
				.RegisterWidgetForWidgetViewModel<CarEventFilterViewModel, CarEventFilterView>()
				.RegisterWidgetForWidgetViewModel<UndeliveredOrdersFilterViewModel, UndeliveredOrdersFilterView>()
				.RegisterWidgetForWidgetViewModel<DriverComplaintReasonJournalFilterViewModel, DriverComplaintReasonJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ComplaintObjectJournalFilterViewModel, ComplaintObjectJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<ComplaintKindJournalFilterViewModel, ComplaintKindJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<WarehousesBalanceSummaryViewModel, WarehousesBalanceSummaryView>()
				.RegisterWidgetForWidgetViewModel<NomenclatureBalanceByStockFilterViewModel, NomenclatureBalanceByStockFilterView>()
				.RegisterWidgetForWidgetViewModel<SalaryByEmployeeJournalFilterViewModel, SalaryByEmployeeFilterView>()
				.RegisterWidgetForWidgetViewModel<ProductGroupJournalFilterViewModel, ProductGroupJournalFilterView>()
				.RegisterWidgetForWidgetViewModel<UndeliveryTransferAbsenceReasonJournalFilterViewModel, UndeliveryTransferAbsenceReasonJournalFilterView>()
				;

			DialogHelper.FilterWidgetResolver = ViewModelWidgetResolver.Instance;
		}

		static void ConfigureJournalColumnsConfigs()
		{
			JournalsColumnsConfigs.RegisterColumns();
		}
		
		static void GetPermissionsSettings() {
			string    sql = "SELECT * FROM permissions_settings";
			DbCommand cmd = QSMain.ConnectionDB.CreateCommand();
			cmd.CommandText = sql;
			using (DbDataReader rdr = cmd.ExecuteReader())
			{
				while (rdr.Read())
				{
					PermissionsSettings.PresetPermissions.Add(rdr["name"].ToString(),
						new PresetUserPermissionSource(rdr["name"].ToString(), rdr["display_name"].ToString(),
							string.IsNullOrEmpty(rdr["description"].ToString()) ? "" : rdr["description"].ToString()));
				}
			}
		}

		public static void CreateTempDir()
		{
			var userId = QSMain.User?.Id;

			if(userId == null)
				return;

			var tempVodUserPath = Path.Combine(Path.GetTempPath(), "Vodovoz", userId.ToString());
			DirectoryInfo dirInfo = new DirectoryInfo(tempVodUserPath);

			if(!dirInfo.Exists) 
				dirInfo.Create();
		}

		public static void ClearTempDir()
		{
			var userId = QSMain.User?.Id;

			if(userId == null)
				return;

			var tempVodUserPath = Path.Combine(Path.GetTempPath(), "Vodovoz", userId.ToString());
			DirectoryInfo dirInfo = new DirectoryInfo(tempVodUserPath);

			if(dirInfo.Exists) 
			{
				foreach(FileInfo file in dirInfo.EnumerateFiles()) {
					file.Delete();
				}
				foreach(DirectoryInfo dir in dirInfo.EnumerateDirectories()) {
					dir.Delete(true);
				}

				dirInfo.Delete();
			}
		}

		public static Autofac.IContainer AppDIContainer;

		static void AutofacClassConfig()
		{
			var builder = new ContainerBuilder();

			#region База
			builder.Register(c => UnitOfWorkFactory.GetDefaultFactory).As<IUnitOfWorkFactory>();
			builder.RegisterType<BaseParametersProvider>().As<ITerminalNomenclatureProvider>().AsSelf();
			#endregion

			#region Сервисы
			
			#region GtkUI
			builder.RegisterType<GtkMessageDialogsInteractive>().As<IInteractiveMessage>();
			builder.RegisterType<GtkQuestionDialogsInteractive>().As<IInteractiveQuestion>();
			builder.RegisterType<GtkInteractiveService>().As<IInteractiveService>();
			#endregion GtkUI
			
			builder.Register(c => ServicesConfig.CommonServices).As<ICommonServices>();
			builder.RegisterType<UserService>().As<IUserService>();
			builder.RegisterType<DeleteEntityGUIService>().As<IDeleteEntityService>();
			builder.Register(c => DeleteConfig.Main).As<DeleteConfiguration>();
			builder.Register(c => PermissionsSettings.CurrentPermissionService).As<ICurrentPermissionService>();
			
			#endregion

			#region Vodovoz
			
			#region Adapters
			
			builder.RegisterType<UndeliveredOrdersJournalOpener>().As<IUndeliveredOrdersJournalOpener>();
			builder.RegisterType<GtkTabsOpener>().As<IGtkTabsOpener>();
			
			#endregion
			
			#region Services
			
			builder.Register(c => VodovozGtkServicesConfig.EmployeeService).As<IEmployeeService>();
			builder.RegisterType<GtkFilePicker>().As<IFilePickerService>();
			builder.Register(c => PermissionExtensionSingletonStore.GetInstance()).As<IPermissionExtensionStore>();
			builder.RegisterType<EntityExtendedPermissionValidator>().As<IEntityExtendedPermissionValidator>();
			builder.RegisterType<EmployeeService>().As<IEmployeeService>();
			builder.RegisterType<ParametersProvider>().As<IParametersProvider>();
			builder.RegisterType<OrderParametersProvider>().As<IOrderParametersProvider>();
			builder.RegisterType<NomenclatureParametersProvider>().As<INomenclatureParametersProvider>();
			builder.Register(c => PermissionsSettings.PermissionService).As<IPermissionService>();

			#endregion
			
			#region Selectors
			
			builder.RegisterType<NomenclatureSelectorFactory>().As<INomenclatureSelectorFactory>();
			builder.RegisterType<OrderSelectorFactory>().As<IOrderSelectorFactory>();
			builder.RegisterType<RdlPreviewOpener>().As<IRDLPreviewOpener>();
			builder.RegisterType<DeliveryPointJournalFactory>().As<IDeliveryPointJournalFactory>();
			builder.RegisterType<EmployeeJournalFactory>().As<IEmployeeJournalFactory>();
			builder.RegisterType<CounterpartyJournalFactory>().As<ICounterpartyJournalFactory>();
			builder.RegisterType<SubdivisionJournalFactory>().As<ISubdivisionJournalFactory>();
			builder.RegisterType<SalesPlanJournalFactory>().As<ISalesPlanJournalFactory>();
			
			#endregion
			
			#region Репозитории

			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(CounterpartyContractRepository)))
				.Where(t => t.Name.EndsWith("Repository"))
				.AsImplementedInterfaces();

			#endregion

			#region Mango
			
			builder.RegisterType<MangoManager>().AsSelf();
			
			#endregion
			
			#endregion

			#region Навигация
			builder.RegisterType<ClassNamesHashGenerator>().As<IPageHashGenerator>();
			builder.Register((ctx) => new AutofacViewModelsTdiPageFactory(AppDIContainer)).As<IViewModelsPageFactory>();
			builder.Register((ctx) => new AutofacTdiPageFactory(AppDIContainer)).As<ITdiPageFactory>();
			builder.Register((ctx) => new AutofacViewModelsGtkPageFactory(AppDIContainer)).AsSelf();
			builder.RegisterType<TdiNavigationManager>().AsSelf().As<INavigationManager>().As<ITdiCompatibilityNavigation>().SingleInstance();
			builder.Register(cc => new ClassNamesBaseGtkViewResolver(typeof(InternalTalkView), typeof(DeletionView))).As<IGtkViewResolver>();
			#endregion

			#region Старые диалоги
			builder.RegisterAssemblyTypes(System.Reflection.Assembly.GetAssembly(typeof(CounterpartyDlg)))
				.Where(t => t.IsAssignableTo<ITdiTab>())
				.AsSelf();
			#endregion

			#region Старые общие диалоги
			builder.RegisterType<ReportViewDlg>().AsSelf();
			#endregion

			#region ViewModels
			
			builder.Register(x => new AutofacViewModelResolver(AppDIContainer)).As<IViewModelResolver>();
			builder.RegisterAssemblyTypes(
					System.Reflection.Assembly.GetAssembly(typeof(InternalTalkViewModel)),
				 	System.Reflection.Assembly.GetAssembly(typeof(ComplaintViewModel)))
				.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
				.AsSelf();
			builder.RegisterType<PrepareDeletionViewModel>().As<IOnCloseActionViewModel>().AsSelf();
			builder.RegisterType<DeletionProcessViewModel>().As<IOnCloseActionViewModel>().AsSelf();

			#endregion

			AppDIContainer = builder.Build();
		}
	}
}
