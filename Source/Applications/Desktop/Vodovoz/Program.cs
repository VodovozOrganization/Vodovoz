using Autofac;
using Autofac.Extensions.DependencyInjection;
using CashReceiptApi.Client.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Deletion;
using QS.Deletion.Configuration;
using QS.Deletion.ViewModels;
using QS.Deletion.Views;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.ErrorReporting;
using QS.ErrorReporting.Handlers;
using QS.Navigation;
using QS.Osrm;
using QS.Permissions;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Project.Services.GtkUI;
using QS.Project.Versioning;
using QS.Report;
using QS.Report.Repository;
using QS.Report.ViewModels;
using QS.Report.Views;
using QS.Services;
using QS.Tdi;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Extension;
using QS.ViewModels.Resolve;
using QS.Views.Resolve;
using QSReport;
using RevenueService.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vodovoz.Additions;
using Vodovoz.Application.Services;
using Vodovoz.Application.Services.Logistics;
using Vodovoz.CachingRepositories.Cash;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.CachingRepositories.Counterparty;
using Vodovoz.Core;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Mango;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalViewers;
using Vodovoz.Models;
using Vodovoz.Models.TrueMark;
using Vodovoz.Parameters;
using Vodovoz.PermissionExtensions;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Reports;
using Vodovoz.Reports.Logistic;
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
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Permissions;
using Vodovoz.Settings.Database;
using Vodovoz.SidePanel.InfoViews;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Logistic;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Mango.Talks;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.Views.Mango.Talks;
using Vodovoz.ViewWidgets;
using VodovozInfrastructure.Endpoints;
using VodovozInfrastructure.Interfaces;
using VodovozInfrastructure.StringHandlers;
using static Vodovoz.ViewModels.Cash.Reports.CashFlowAnalysisViewModel;
using IErrorReporter = Vodovoz.Tools.IErrorReporter;

namespace Vodovoz
{
	public class Program
	{
		private static string _nLogSectionName = nameof(NLog);

		[STAThread]
		public static void Main(string[] args)
		{
			try
			{
				Gtk.Application.Init();

				var host = CreateHostBuilder().Build();

				host.Services.GetService<Startup>().Start(args);
			}
			finally
			{
				// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
				NLog.LogManager.Shutdown();
			}
		}

		public static IHostBuilder CreateHostBuilder() =>
			new HostBuilder()
				.ConfigureAppConfiguration((hostingContext, config) =>
				{
					config.AddJsonFile("appsettings.json");
				})
				.ConfigureLogging((hostContext, logging) =>
				{
					logging.ClearProviders();
					logging.AddNLog();
					logging.AddConfiguration(hostContext.Configuration.GetSection(_nLogSectionName));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory(builder =>
				{
					builder.RegisterType<ApplicationVersionInfo>().As<IApplicationInfo>().SingleInstance();

					builder.Register(c => new ErrorReportingSettings(false, false, true, 100))
						.As<IErrorReportingSettings>()
						.SingleInstance();

					builder.RegisterType<LogService>().As<ILogService>().SingleInstance();

					#region База

					builder.Register(c => UnitOfWorkFactory.GetDefaultFactory).As<IUnitOfWorkFactory>();
					builder.Register(c => Startup.DataBaseInfo).As<IDataBaseInfo>().SingleInstance();

					#endregion

					#region Репозитории

					builder.RegisterType<UserPrintingRepository>().As<IUserPrintingRepository>().SingleInstance();
					builder.RegisterType<CashRepository>().As<ICashRepository>();

					#endregion

					#region Сервисы

					//GtkUI
					builder.RegisterType<GtkMessageDialogsInteractive>().As<IInteractiveMessage>();
					builder.RegisterType<GtkQuestionDialogsInteractive>().As<IInteractiveQuestion>();
					builder.RegisterType<GtkInteractiveService>().As<IInteractiveService>();

					builder.Register(c => ServicesConfig.CommonServices).As<ICommonServices>();
					builder.Register(с => ServicesConfig.UserService).As<IUserService>();
					builder.RegisterType<DeleteEntityGUIService>().As<IDeleteEntityService>();
					builder.Register(c => DeleteConfig.Main).As<DeleteConfiguration>();
					builder.Register(c => PermissionsSettings.CurrentPermissionService).As<ICurrentPermissionService>();
					builder.RegisterType<ReportPrinter>().As<IReportPrinter>();

					builder.RegisterType<EntityDeleteWorker>().AsSelf().As<IEntityDeleteWorker>();
					builder.RegisterType<CommonMessages>().AsSelf();

					#endregion

					#region Старые общие диалоги

					builder.RegisterType<ReportViewDlg>().AsSelf();

					#endregion

					#region Навигация

					builder.RegisterType<ClassNamesHashGenerator>().As<IPageHashGenerator>();
					builder.Register(context => new AutofacViewModelsTdiPageFactory(context.Resolve<ILifetimeScope>())).As<IViewModelsPageFactory>();
					builder.Register(context => new AutofacTdiPageFactory(context.Resolve<ILifetimeScope>())).As<ITdiPageFactory>();
					builder.Register(context => new AutofacViewModelsGtkPageFactory(context.Resolve<ILifetimeScope>())).AsSelf();
					builder.RegisterType<TdiNavigationManagerAdapter>().AsSelf().As<INavigationManager>().As<ITdiCompatibilityNavigation>()
						.SingleInstance();
					builder.Register(context => new ClassNamesBaseGtkViewResolver(context.Resolve<IGtkViewFactory>(),
						typeof(InternalTalkView),
						typeof(DeletionView),
						typeof(RdlViewerView))
					).As<IGtkViewResolver>();

					#endregion

					#region ViewModels

					builder.Register(context => new AutofacViewModelResolver(context.Resolve<ILifetimeScope>())).As<IViewModelResolver>();
					builder.Register(с => NotifyConfiguration.Instance).As<IEntityChangeWatcher>();
					builder.RegisterAssemblyTypes(
							Assembly.GetAssembly(typeof(InternalTalkViewModel)),
							Assembly.GetAssembly(typeof(ComplaintViewModel)))
						.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
						.AsSelf();
					builder.RegisterType<PrepareDeletionViewModel>().As<IOnCloseActionViewModel>().AsSelf();
					builder.RegisterType<DeletionProcessViewModel>().As<IOnCloseActionViewModel>().AsSelf();
					builder.RegisterType<DeletionViewModel>().AsSelf();
					builder.RegisterType<RdlViewerViewModel>().AsSelf();
					builder.RegisterType<ProgressWindowViewModel>().AsSelf();

					#endregion

					#region Обработчики ошибок

					builder.RegisterType<MySqlException1055OnlyFullGroupBy>().AsSelf();
					builder.RegisterType<MySqlException1366IncorrectStringValue>().AsSelf();
					builder.RegisterType<NHibernateFlushAfterException>().AsSelf();

					#endregion Обработчики ошибок

					// Классы водовоза

					builder.RegisterType<WaterFixedPricesGenerator>().AsSelf();
					builder.Register(c => ViewModelWidgetResolver.Instance)
						.AsSelf()
						.As<ITDIWidgetResolver>()
						.As<IFilterWidgetResolver>()
						.As<IWidgetResolver>()
						.As<IGtkViewResolver>()
						.SingleInstance();

					builder.RegisterType<TrueMarkCodesPool>()
						.AsSelf()
						.InstancePerLifetimeScope();

					builder.RegisterType<TrueMarkCodePoolLoader>()
						.AsSelf()
						.InstancePerLifetimeScope();

					builder.RegisterType<TrueMarkWaterCodeParser>()
						.AsSelf()
						.InstancePerLifetimeScope();

					builder.RegisterType<ReceiptManualController>()
						.AsSelf()
						.InstancePerLifetimeScope();

					builder.RegisterType<FiscalizationResultSaver>()
						.AsSelf()
						.InstancePerLifetimeScope();

					builder.RegisterModule<DatabaseSettingsModule>();
					builder.RegisterModule<CashReceiptClientChannelModule>();

					builder.RegisterType<FileChooser>().As<IFileChooserProvider>();


					#region Adapters & Factories

					builder.RegisterType<GtkTabsOpener>().As<IGtkTabsOpener>();
					builder.RegisterType<RdlPreviewOpener>().As<IRDLPreviewOpener>();
					builder.RegisterType<GtkReportViewOpener>().As<IReportViewOpener>().SingleInstance();
					builder.RegisterType<RoboatsJournalsFactory>().AsSelf().InstancePerLifetimeScope();

					builder.RegisterAssemblyTypes(
							Assembly.GetExecutingAssembly(),
							Assembly.GetAssembly(typeof(VodovozBusinessAssemblyFinder)),
							Assembly.GetAssembly(typeof(VodovozViewModelAssemblyFinder)))
						.Where(t => t.Name.EndsWith("Factory")
							&& t.GetInterfaces()
								.Where(i => i.Name == $"I{t.Name}")
								.FirstOrDefault() != null)
						.As((s) => s.GetTypeInfo()
							.GetInterfaces()
							.Where(i => i.Name == $"I{s.Name}")
							.First())
						.SingleInstance();

					builder.RegisterType<IncludeExcludeSalesFilterFactory>().As<IIncludeExcludeSalesFilterFactory>().InstancePerLifetimeScope();
					builder.RegisterType<LeftRightListViewModelFactory>().As<ILeftRightListViewModelFactory>().InstancePerLifetimeScope();

					#endregion

					#region Controllers

					builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(VodovozBusinessAssemblyFinder)))
						.Where(t => (t.Name.EndsWith("Controller") || t.Name.EndsWith("Handler"))
							&& t.GetInterfaces()
								.Where(i => i.Name == $"I{t.Name}")
								.FirstOrDefault() != null)
						.As((s) => s.GetTypeInfo()
							.GetInterfaces()
							.Where(i => i.Name == $"I{s.Name}")
							.First());

					builder.RegisterType<GeoGroupVersionsModel>().SingleInstance().AsSelf();
					builder.RegisterType<NomenclatureFixedPriceController>().As<INomenclatureFixedPriceProvider>().AsSelf();
					builder.RegisterType<StringHandler>().As<IStringHandler>();

					#endregion

					#region Services

					builder.Register(c => VodovozGtkServicesConfig.EmployeeService).As<IEmployeeService>();
					builder.RegisterType<FileDialogService>().As<IFileDialogService>();
					builder.Register(c => PermissionExtensionSingletonStore.GetInstance()).As<IPermissionExtensionStore>();
					builder.RegisterType<EntityExtendedPermissionValidator>().As<IEntityExtendedPermissionValidator>();
					builder.RegisterType<EmployeeService>().As<IEmployeeService>();
					builder.Register(c => PermissionsSettings.PermissionService).As<IPermissionService>();
					builder.Register(c => ErrorReporter.Instance).As<IErrorReporter>();
					builder.RegisterType<ObjectValidator>().As<IValidator>().AsSelf();
					builder.RegisterType<WarehousePermissionService>().As<IWarehousePermissionService>().AsSelf();
					builder.RegisterType<UsersPresetPermissionValuesGetter>().AsSelf();
					builder.RegisterType<UsersEntityPermissionValuesGetter>().AsSelf();
					builder.RegisterType<UserPermissionsExporter>().AsSelf();
					builder.RegisterType<AuthorizationService>().As<IAuthorizationService>();
					builder.RegisterType<UserSettingsGetter>().As<IUserSettings>();
					builder.RegisterType<StoreDocumentHelper>().AsSelf();
					builder.RegisterType<WarehousePermissionValidator>().As<IWarehousePermissionValidator>();
					builder.RegisterType<WageParameterService>().As<IWageParameterService>();
					builder.RegisterType<SelfDeliveryCashOrganisationDistributor>().As<ISelfDeliveryCashOrganisationDistributor>();

					builder.RegisterType<CounterpartyService>().As<ICounterpartyService>().InstancePerLifetimeScope();

					#endregion

					#region Models

					builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(VodovozBusinessAssemblyFinder)))
						.Where(t => t.Name.EndsWith("Model") && !t.Name.EndsWith("ViewModel")
							&& t.GetInterfaces()
								.Where(i => i.Name == $"I{t.Name}")
								.FirstOrDefault() != null)
						.As((s) => s.GetTypeInfo()
							.GetInterfaces()
							.Where(i => i.Name == $"I{s.Name}")
							.First());

					builder.RegisterType<WageParameterService>().AsSelf();
					builder.RegisterType<UserWarehousePermissionModel>()
						.As<WarehousePermissionModelBase>()
						.AsSelf();

					#endregion

					#region CallTasks

					builder.Register(context => CallTaskSingletonFactory.GetInstance()).As<ICallTaskFactory>();

					builder.RegisterType<CallTaskWorker>().As<ICallTaskWorker>();

					#endregion

					#region Репозитории

					builder.RegisterGeneric(typeof(GenericRepository<>)).As(typeof(IGenericRepository<>)).InstancePerLifetimeScope();

					builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(CounterpartyContractRepository)))
						.Where(t => t.Name.EndsWith("Repository")
							&& t.GetInterfaces()
								.Where(i => i.Name == $"I{t.Name}")
								.FirstOrDefault() != null)
						.As((s) => s.GetTypeInfo()
							.GetInterfaces()
							.Where(i => i.Name == $"I{s.Name}")
							.First())
						.SingleInstance();

					#endregion

					#region Кэширующие репозитории

					builder.RegisterType<FinancialExpenseCategoriesNodesInMemoryCacheRepository>()
						.As<IDomainEntityNodeInMemoryCacheRepository<FinancialExpenseCategory>>();

					builder.RegisterType<FinancialIncomeCategoriesNodesInMemoryCacheRepository>()
						.As<IDomainEntityNodeInMemoryCacheRepository<FinancialIncomeCategory>>();

					builder.RegisterType<CounterpartyInMemoryTitlesCacheRepository>()
						.As<IDomainEntityNodeInMemoryCacheRepository<Counterparty>>();

					#endregion Кэширующие репозитории

					#region Mango

					builder.RegisterType<MangoManager>().AsSelf();

					#endregion

					#region Reports

					builder.RegisterType<CounterpartyCashlessDebtsReport>().AsSelf();
					builder.RegisterType<OrderChangesReport>().AsSelf();
					builder.RegisterType<CashFlow>().AsSelf();
					builder.RegisterType<WayBillReportGroupPrint>().AsSelf();
					builder.RegisterType<StockMovements>().AsSelf();
					builder.RegisterType<SalaryRatesReport>().AsSelf();
					builder.RegisterType<AnalyticsForUndeliveryReport>().AsSelf();
					builder.RegisterType<PaymentsFromAvangardReport>().AsSelf();
					builder.RegisterType<EmployeesTaxesSumReport>().AsSelf();
					builder.RegisterType<EmployeesFines>().AsSelf();
					builder.RegisterType<SalesReportView>().AsSelf();
					builder.RegisterType<SalesByDiscountReport>().AsSelf();
					builder.RegisterType<DriverWagesReport>().AsSelf();
					builder.RegisterType<FuelReport>().AsSelf();
					builder.RegisterType<ShortfallBattlesReport>().AsSelf();
					builder.RegisterType<WagesOperationsReport>().AsSelf();
					builder.RegisterType<EquipmentReport>().AsSelf();
					builder.RegisterType<ForwarderWageReport>().AsSelf();
					builder.RegisterType<CashierCommentsReport>().AsSelf();
					builder.RegisterType<OnecCommentsReport>().AsSelf();
					builder.RegisterType<DriversWageBalanceReport>().AsSelf();
					builder.RegisterType<DeliveriesLateReport>().AsSelf();
					builder.RegisterType<QualityReport>().AsSelf();
					builder.RegisterType<ProducedProductionReport>().AsSelf();
					builder.RegisterType<DriverRoutesListRegisterReport>().AsSelf();
					builder.RegisterType<RoutesListRegisterReport>().AsSelf();
					builder.RegisterType<DeliveryTimeReport>().AsSelf();
					builder.RegisterType<OrdersByDistrictReport>().AsSelf();
					builder.RegisterType<CompanyTrucksReport>().AsSelf();
					builder.RegisterType<LastOrderByDeliveryPointReport>().AsSelf();
					builder.RegisterType<OrderIncorrectPrices>().AsSelf();
					builder.RegisterType<OrdersWithMinPriceLessThan>().AsSelf();
					builder.RegisterType<RouteListsOnClosingReport>().AsSelf();
					builder.RegisterType<OnLoadTimeAtDayReport>().AsSelf();
					builder.RegisterType<SelfDeliveryReport>().AsSelf();
					builder.RegisterType<ShipmentReport>().AsSelf();
					builder.RegisterType<BottlesMovementReport>().AsSelf();
					builder.RegisterType<MileageReport>().AsSelf();
					builder.RegisterType<MastersReport>().AsSelf();
					builder.RegisterType<SuburbWaterPriceReport>().AsSelf();
					builder.RegisterType<BottlesMovementSummaryReport>().AsSelf();
					builder.RegisterType<DrivingCallReport>().AsSelf();
					builder.RegisterType<MastersVisitReport>().AsSelf();
					builder.RegisterType<NotDeliveredOrdersReport>().AsSelf();
					builder.RegisterType<EmployeesPremiums>().AsSelf();
					builder.RegisterType<OrderStatisticByWeekReport>().AsSelf();
					builder.RegisterType<ReportForBigClient>().AsSelf();
					builder.RegisterType<OrderRegistryReport>().AsSelf();
					builder.RegisterType<EquipmentBalance>().AsSelf();
					builder.RegisterType<CardPaymentsOrdersReport>().AsSelf();
					builder.RegisterType<DefectiveItemsReport>().AsSelf();
					builder.RegisterType<PaymentsFromTinkoffReport>().AsSelf();
					builder.RegisterType<OrdersByDistrictsAndDeliverySchedulesReport>().AsSelf();
					builder.RegisterType<OrdersByCreationDateReport>().AsSelf();
					builder.RegisterType<NomenclatureForShipment>().AsSelf();
					builder.RegisterType<OrderCreationDateReport>().AsSelf();
					builder.RegisterType<NotFullyLoadedRouteListsReport>().AsSelf();
					builder.RegisterType<FirstClientsReport>().AsSelf();
					builder.RegisterType<TariffZoneDebts>().AsSelf();
					builder.RegisterType<ClientsByDeliveryPointCategoryAndActivityKindsReport>().AsSelf();
					builder.RegisterType<ExtraBottleReport>().AsSelf();
					builder.RegisterType<FirstSecondClientReport>().AsSelf();
					builder.RegisterType<FuelConsumptionReport>().AsSelf();
					builder.RegisterType<CounterpartyCloseDeliveryReport>().AsSelf();
					builder.RegisterType<IncomeBalanceReport>().AsSelf();
					builder.RegisterType<CashBookReport>().AsSelf();
					builder.RegisterType<ProfitabilityBottlesByStockReport>().AsSelf();
					builder.RegisterType<PlanImplementationReport>().AsSelf();
					builder.RegisterType<ZeroDebtClientReport>().AsSelf();
					builder.RegisterType<OrdersCreationTimeReport>().AsSelf();
					builder.RegisterType<PotentialFreePromosetsReport>().AsSelf();
					builder.RegisterType<PaymentsFromBankClientFinDepartmentReport>().AsSelf();
					builder.RegisterType<ChainStoreDelayReport>().AsSelf();
					builder.RegisterType<ReturnedTareReport>().AsSelf();
					builder.RegisterType<ProductionRequestReport>().AsSelf();
					builder.RegisterType<FuelConsumptionReport>().AsSelf();
					builder.RegisterType<NonClosedRLByPeriodReport>().AsSelf();
					builder.RegisterType<EShopSalesReport>().AsSelf();
					builder.RegisterType<CounterpartyReport>().AsSelf();
					builder.RegisterType<DriversToDistrictsAssignmentReport>().AsSelf();
					builder.RegisterType<GeneralSalaryInfoReport>().AsSelf();
					builder.RegisterType<EmployeesReport>().AsSelf();
					builder.RegisterType<AddressesOverpaymentsReport>().AsSelf();
					builder.RegisterType<StockMovementsAdvancedReport>().AsSelf();

					#endregion

					#region Старые диалоги

					builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(CounterpartyDlg)))
						.Where(t => t.IsAssignableTo<ITdiTab>())
						.AsSelf();

					#endregion

					#region ParameterProviders

					builder.RegisterType<BaseParametersProvider>()
						.As<IStandartNomenclatures>()
						.As<IImageProvider>()
						.As<IStandartDiscountsService>()
						.As<IPersonProvider>()
						.As<IWageParametersProvider>()
						.As<ISmsNotifierParametersProvider>()
						.As<IWageParametersProvider>()
						.As<IDefaultDeliveryDayScheduleSettings>()
						.As<ISmsNotificationServiceSettings>()
						.As<ISalesReceiptsServiceSettings>()
						.As<IEmailServiceSettings>()
						.As<IDriverServiceParametersProvider>()
						.As<IErrorSendParameterProvider>()
						.As<IProfitCategoryProvider>()
						.As<IPotentialFreePromosetsReportDefaultsProvider>()
						.As<IMailjetParametersProvider>()
						.As<IVpbxSettings>()
						.As<ITerminalNomenclatureProvider>()
						.AsSelf();

					builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(ParametersProvider)))
						.Where(t => t.Name.EndsWith("Provider")
							&& t.GetInterfaces()
								.Where(i => i.Name == $"I{t.Name}")
								.FirstOrDefault() != null)
						.As((s) => s.GetTypeInfo()
							.GetInterfaces()
							.Where(i => i.Name == $"I{s.Name}")
							.First())
						.SingleInstance();

					builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(ParametersProvider)))
						.Where(t => t.Name.EndsWith("Settings")
							&& t.GetInterfaces()
								.Where(i => i.Name == $"I{t.Name}")
								.FirstOrDefault() != null)
						.As((s) => s.GetTypeInfo()
							.GetInterfaces()
							.Where(i => i.Name == $"I{s.Name}")
							.First())
						.SingleInstance();

					#endregion

					#region Фильтры

					builder.RegisterType<PaymentsJournalFilterViewModel>().AsSelf();
					builder.RegisterType<UnallocatedBalancesJournalFilterViewModel>().AsSelf();
					builder.RegisterType<SelectableParametersReportFilter>().AsSelf();

					#endregion

					#region Классы

					builder.RegisterType<IncludeExludeFiltersViewModel>().AsSelf();

					builder.RegisterType<User>().AsSelf();
					builder.RegisterType<EntitySubdivisionForUserPermission>().AsSelf();
					builder.RegisterType<EntityUserPermissionExtended>().AsSelf();
					builder.RegisterType<EntityUserPermission>().AsSelf();
					builder.RegisterType<HierarchicalPresetUserPermission>().AsSelf();
					builder.RegisterType<UserWarehousePermission>().AsSelf();
					builder.RegisterType<EntityUserPermissionExtended>().AsSelf();
					builder.RegisterType<UserPermissionNode>()
						.AsSelf()
						.As<IPermissionNode>();

					builder.Register(context =>
						{
							var cs = new ConfigurationSection(
							new ConfigurationRoot(
								new List<IConfigurationProvider>
								{
							new MemoryConfigurationProvider(new MemoryConfigurationSource())
								}
								), "");

							cs["BaseUri"] = "https://driverapi.vod.qsolution.ru:7090/api/v2/";

							var clientProvider = new ApiClientProvider.ApiClientProvider(cs);

							return new DriverApiUserRegisterEndpoint(clientProvider);
						}
						).As<DriverApiUserRegisterEndpoint>();

					builder.Register(c => CurrentUserSettings.Settings).As<UserSettings>();

					builder.RegisterType<PasswordGenerator>().As<IPasswordGenerator>();

					builder.RegisterType<StoreDocumentHelper>().As<IStoreDocumentHelper>();

					builder.RegisterType<AdvanceCashOrganisationDistributor>().As<IAdvanceCashOrganisationDistributor>();

					builder.RegisterType<RouteListCashOrganisationDistributor>().As<IRouteListCashOrganisationDistributor>();

					builder.RegisterType<IncomeCashOrganisationDistributor>().As<IIncomeCashOrganisationDistributor>();

					builder.RegisterType<ExpenseCashOrganisationDistributor>().As<IExpenseCashOrganisationDistributor>();

					builder.RegisterType<FuelCashOrganisationDistributor>().As<IFuelCashOrganisationDistributor>();

					builder.RegisterType<StoreDocumentHelper>().As<IStoreDocumentHelper>();

					builder.RegisterType<CashFlowDdsReportRenderer>().AsSelf();

					builder.Register((context) =>
					{
						var counterpartySettings = context.Resolve<ICounterpartySettings>();

						return new RevenueServiceClient(counterpartySettings.RevenueServiceClientAccessToken);
					}).As<IRevenueServiceClient>().InstancePerLifetimeScope();

					#endregion

					#region InfoPanelViews

					builder.RegisterType<CarsMonitoringInfoPanelView>().AsSelf();

					#endregion
				}))
				.ConfigureServices((hostingContext, services) =>
				{
					services.AddSingleton<Startup>()
							.AddScoped<IRouteListService, RouteListService>()
							.AddScoped<RouteGeometryCalculator>()
							.AddSingleton<OsrmClient>(sp => OsrmClientFactory.Instance)
							.AddSingleton<IFastDeliveryDistanceChecker, DistanceCalculator>();
				});
	}
}
