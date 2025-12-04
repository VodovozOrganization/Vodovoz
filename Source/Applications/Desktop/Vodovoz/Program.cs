using Autofac;
using Autofac.Extensions.DependencyInjection;
using CashReceiptApi.Client.Framework;
using Edo.Common;
using EdoService.Library;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Pacs.Core;
using Pacs.Operators.Client;
using QS.Deletion;
using QS.Deletion.Configuration;
using QS.Deletion.ViewModels;
using QS.Dialog;
using QS.Dialog.GtkUI.FileDialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.NotifyChange;
using QS.ErrorReporting;
using QS.ErrorReporting.Handlers;
using QS.Navigation;
using QS.Permissions;
using QS.Project.Domain;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Project.Versioning;
using QS.Report;
using QS.Report.Repository;
using QS.Report.ViewModels;
using QS.Services;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.ViewModels;
using QS.ViewModels.Extension;
using QS.ViewModels.Resolve;
using QSReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QS.Project.DB.Passwords;
using Vodovoz.Additions;
using Vodovoz.Additions.Printing;
using Vodovoz.Application.Mango;
using Vodovoz.Application.Pacs;
using Vodovoz.CachingRepositories.Cash;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.CachingRepositories.Counterparty;
using Vodovoz.Configuration;
using Vodovoz.Core;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Sms;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Models;
using Vodovoz.Models.TrueMark;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Presentation.ViewModels.Mango;
using Vodovoz.Presentation.ViewModels.Pacs;
using Vodovoz.Reports;
using Vodovoz.Reports.Logistic;
using Vodovoz.ReportsParameters;
using Vodovoz.ReportsParameters.Bookkeeping;
using Vodovoz.ReportsParameters.Bottles;
using Vodovoz.ReportsParameters.Employees;
using Vodovoz.ReportsParameters.Logistic;
using Vodovoz.ReportsParameters.Orders;
using Vodovoz.ReportsParameters.Payments;
using Vodovoz.ReportsParameters.Retail;
using Vodovoz.ReportsParameters.Sales;
using Vodovoz.ReportsParameters.Store;
using Vodovoz.Services;
using Vodovoz.Services.Permissions;
using Vodovoz.SidePanel.InfoViews;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Interactive.ConfirmationQuestion;
using Vodovoz.Tools.Interactive.YesNoCancelQuestion;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs.Mango;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Mango;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewWidgets;
using VodovozInfrastructure.Configuration;
using VodovozInfrastructure.Endpoints;
using VodovozInfrastructure.Interfaces;
using VodovozInfrastructure.Passwords;
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
				host.RunAsync();
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
					config
						.AddJsonFile("appsettings.json", false, true)
						.AddJsonFile("appsettings.Production.json", true, true);
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

					#region Репозитории

					builder.RegisterType<UserPrintingRepository>().As<IUserPrintingRepository>().SingleInstance();

					#endregion

					#region Сервисы

					//GtkUI
					builder.RegisterType<GtkConfirmationQuestionInteractive>().As<IConfirmationQuestionInteractive>();
					builder.RegisterType<GtkYesNoCancelQuestionInteractive>().As<IYesNoCancelQuestionInteractive>();

					builder.Register(c => ServicesConfig.CommonServices).As<ICommonServices>();
					builder.RegisterType<DeleteEntityGUIService>().AsSelf().As<IDeleteEntityService>();
					builder.Register(c => DeleteConfig.Main).As<DeleteConfiguration>();
					builder.Register(c => PermissionsSettings.CurrentPermissionService).As<ICurrentPermissionService>();
					builder.RegisterType<ReportPrinter>().As<IReportPrinter>();
					builder.RegisterType<CustomPrintRdlDocumentsPrinter>().As<ICustomPrintRdlDocumentsPrinter>();

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
					builder.Register<TdiNotebook>((context) => TDIMain.MainNotebook);
					builder.RegisterType<TdiNavigationManagerAdapter>().AsSelf().As<INavigationManager>().As<ITdiCompatibilityNavigation>()
						.SingleInstance();

					#endregion

					#region ViewModels

					builder.Register(context => new AutofacViewModelResolver(context.Resolve<ILifetimeScope>())).As<IViewModelResolver>();
					builder.Register(с => NotifyConfiguration.Instance).As<IEntityChangeWatcher>();
					builder.RegisterAssemblyTypes(
							Assembly.GetExecutingAssembly(),
							Assembly.GetAssembly(typeof(ComplaintViewModel)),
							Assembly.GetAssembly(typeof(PacsPanelViewModel)))
						.Where(t => t.IsAssignableTo<ViewModelBase>() && t.Name.EndsWith("ViewModel"))
						.AsSelf();
					builder.RegisterType<PrepareDeletionViewModel>().As<IOnCloseActionViewModel>().AsSelf();
					builder.RegisterType<DeletionProcessViewModel>().As<IOnCloseActionViewModel>().AsSelf();
					builder.RegisterType<DeletionViewModel>().AsSelf();
					builder.RegisterType<RdlViewerViewModel>().AsSelf();
					builder.RegisterType<ProgressWindowViewModel>().AsSelf();
					builder.RegisterType<PacsViewModelFactory>().As<IPacsViewModelFactory>();

					#endregion

					#region Обработчики ошибок

					builder.RegisterType<MySqlException1055OnlyFullGroupBy>().AsSelf();
					builder.RegisterType<MySqlException1366IncorrectStringValue>().AsSelf();
					builder.RegisterType<NHibernateFlushAfterException>().AsSelf();

					#endregion Обработчики ошибок

					// Классы водовоза

					builder.RegisterType<WaterFixedPricesGenerator>().AsSelf();

					builder.RegisterType<RouteListDailyNumberProvider>()
						.As<IRouteListDailyNumberProvider>();

					# region TrueMark

					builder.RegisterType<Models.TrueMark.TrueMarkCodesPool>()
						.AsSelf()
						.InstancePerLifetimeScope();

					builder.RegisterType<TrueMarkCodePoolLoader>()
						.AsSelf()
						.InstancePerLifetimeScope();

					builder.RegisterType<TrueMarkCodesChecker>()
						.AsSelf()
						.InstancePerLifetimeScope();

					builder.RegisterType<TrueMarkTaskCodesValidator>()
						.As<ITrueMarkCodesValidator>()
						.InstancePerLifetimeScope();

					builder.RegisterType<TrueMarkWaterCodeParser>()
						.AsSelf()
						.InstancePerLifetimeScope();

					#endregion TrueMark
					
					builder.RegisterType<ReceiptManualController>()
						.AsSelf()
						.InstancePerLifetimeScope();

					builder.RegisterType<FiscalizationResultSaver>()
						.AsSelf()
						.InstancePerLifetimeScope();

					builder.RegisterModule<CashReceiptClientChannelModule>();

					builder.RegisterType<OperatorStateMachine>().As<IOperatorStateMachine>();
					builder.RegisterType<OperatorClientFactory>().As<IOperatorClientFactory>();
					builder.RegisterType<OperatorClient>().As<IOperatorClient>();

					builder.RegisterType<PacsDashboardModel>()
						.AsSelf()
						.As<IObserver<OperatorState>>()
						.As<IObserver<Pacs.Core.Messages.Events.PacsCallEvent>>();

					builder.RegisterType<PacsDashboardViewModelFactory>().As<IPacsDashboardViewModelFactory>()
						.SingleInstance();
					
					builder.RegisterType<PacsEmployeeProvider>()
						.As<IPacsEmployeeProvider>()
						.As<IPacsOperatorProvider>()
						.As<IPacsAdministratorProvider>()
						.InstancePerLifetimeScope();

					builder.RegisterType<FileChooser>().As<IFileChooserProvider>();
					builder.RegisterType<SmsNotifier>().As<ISmsNotifier>();

					#region Adapters & Factories

					builder.RegisterType<GtkTabsOpener>().As<IGtkTabsOpener>();
					builder.RegisterType<RdlPreviewOpener>().As<IRDLPreviewOpener>();
					builder.RegisterType<GtkReportViewOpener>().As<IReportViewOpener>().SingleInstance();
					builder.RegisterType<RoboatsJournalsFactory>().AsSelf().InstancePerLifetimeScope();

					builder.RegisterAssemblyTypes(
							Assembly.GetExecutingAssembly(),
							Assembly.GetAssembly(typeof(VodovozViewModelAssemblyFinder)),
							Assembly.GetAssembly(typeof(Vodovoz.Presentation.ViewModels.AssemblyFinder)))
						.Where(t => t.Name.EndsWith("Factory")
							&& t.GetInterfaces()
								.Where(i => i.Name == $"I{t.Name}")
								.FirstOrDefault() != null)
						.As((s) => s.GetTypeInfo()
							.GetInterfaces()
							.Where(i => i.Name == $"I{s.Name}")
							.First())
						.InstancePerLifetimeScope();

					builder.RegisterType<IncludeExcludeSalesFilterFactory>().As<IIncludeExcludeSalesFilterFactory>().InstancePerLifetimeScope();
					builder.RegisterType<LeftRightListViewModelFactory>().As<ILeftRightListViewModelFactory>().InstancePerLifetimeScope();
					builder.RegisterType<PacsViewModelOpener>().As<IPacsViewModelOpener>().InstancePerLifetimeScope();

					#endregion

					#region Controllers

					builder.RegisterType<GeoGroupVersionsModel>().SingleInstance().AsSelf();
					builder.RegisterType<StringHandler>().As<IStringHandler>();

					#endregion

					#region Services

					builder.RegisterType<EmployeeService>().As<IEmployeeService>();
					builder.RegisterType<FileDialogService>().As<IFileDialogService>();
					builder.Register(c => ErrorReporter.Instance).As<IErrorReporter>();
					builder.RegisterType<WarehousePermissionService>().As<IWarehousePermissionService>().AsSelf();
					builder.RegisterType<UsersPresetPermissionValuesGetter>().AsSelf();
					builder.RegisterType<UsersEntityPermissionValuesGetter>().AsSelf();
					builder.RegisterType<UserPermissionsExporter>().AsSelf();
					builder.RegisterType<AuthorizationService>().As<IAuthorizationService>();
					builder.RegisterType<UserSettingsService>().As<IUserSettingsService>();
					builder.RegisterType<StoreDocumentHelper>().AsSelf();
					builder.RegisterType<WageParameterService>().As<IWageParameterService>();
					builder.RegisterType<SelfDeliveryCashOrganisationDistributor>().As<ISelfDeliveryCashOrganisationDistributor>();
					builder.RegisterType<EdoService.Library.EdoService>().As<IEdoService>();
					builder.RegisterType<EmailService>().As<IEmailService>();
					builder.RegisterType<MysqlChangePasswordModelExtended>().As<IChangePasswordModel>();
					builder.RegisterType<ApplicationConfigurator>().As<IApplicationConfigurator>().SingleInstance();

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

					#region Кэширующие репозитории

					builder.RegisterType<FinancialExpenseCategoriesNodesInMemoryCacheRepository>()
						.As<IDomainEntityNodeInMemoryCacheRepository<FinancialExpenseCategory>>();

					builder.RegisterType<FinancialIncomeCategoriesNodesInMemoryCacheRepository>()
						.As<IDomainEntityNodeInMemoryCacheRepository<FinancialIncomeCategory>>();

					builder.RegisterType<CounterpartyInMemoryTitlesCacheRepository>()
						.As<IDomainEntityNodeInMemoryCacheRepository<Counterparty>>();

					#endregion Кэширующие репозитории

					#region Mango

					builder.RegisterType<MangoViewModelNavigator>()
						.As<IMangoViewModelNavigator>()
						.SingleInstance();

					builder.RegisterType<MangoManager>()
						.As<IMangoManager>()
						.AsSelf()
						.SingleInstance();

					#endregion

					#region Reports

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
					builder.RegisterType<QualityReport>().AsSelf();
					builder.RegisterType<DriverRoutesListRegisterReport>().AsSelf();
					builder.RegisterType<RoutesListRegisterReport>().AsSelf();
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
					builder.RegisterType<OrdersByDistrictsAndDeliverySchedulesReport>().AsSelf();
					builder.RegisterType<OrdersByCreationDateReport>().AsSelf();
					builder.RegisterType<NomenclatureForShipment>().AsSelf();
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
					builder.RegisterType<AddressesOverpaymentsReport>().AsSelf();
					builder.RegisterType<StockMovementsAdvancedReport>().AsSelf();

					#endregion

					#region Старые диалоги

					builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(CounterpartyDlg)))
						.Where(t => t.IsAssignableTo<ITdiTab>())
						.AsSelf();

					#endregion

					#region Фильтры

					builder.RegisterType<PaymentsJournalFilterViewModel>().AsSelf();
					builder.RegisterType<UnallocatedBalancesJournalFilterViewModel>().AsSelf();
					builder.RegisterType<SelectableParametersReportFilter>().AsSelf();
					builder.RegisterType<RequestsToSuppliersFilterViewModel>().AsSelf();

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

							cs["BaseUri"] = "https://driverapi.vod.qsolution.ru:7090/api/v5/";

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

					#endregion

					#region InfoPanelViews

					builder.RegisterType<CarsMonitoringInfoPanelView>().AsSelf();

					#endregion
				}))
				.ConfigureServices((hostingContext, services) =>
				{
					services.AddWaterDeliveryDesktop(hostingContext.Configuration);
				});
	}
}
