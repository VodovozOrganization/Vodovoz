using Autofac;
using CashReceiptApi.Client.Framework;
using DriverApi.Notifications.Client;
using Edo.Common;
using Edo.Transport;
using EdoService.Library;
using Fias.Client;
using FuelControl.Library;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;
using MySqlConnector;
using Pacs.Admin.Client;
using Pacs.Admin.Client.Consumers;
using Pacs.Admin.Client.Consumers.Definitions;
using Pacs.Calls.Consumers;
using Pacs.Calls.Consumers.Definitions;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Client;
using Pacs.Operators.Client.Consumers;
using Pacs.Operators.Client.Consumers.Definitions;
using QS.Attachments;
using QS.Deletion;
using QS.Deletion.Configuration;
using QS.Deletion.ViewModels;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.Dialog.ViewModels;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.Entity.PresetPermissions;
using QS.DomainModel.NotifyChange;
using QS.ErrorReporting;
using QS.ErrorReporting.Handlers;
using QS.HistoryLog;
using QS.Navigation;
using QS.Osrm;
using QS.Permissions;
using QS.Project;
using QS.Project.Core;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.GtkSharp;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Project.Versioning;
using QS.Report;
using QS.Report.Repository;
using QS.Report.ViewModels;
using QS.Services;
using QS.Tdi;
using QS.Tdi.Gtk;
using QS.Validation;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Resolve;
using QS.Views.Resolve;
using QSAttachment;
using QSProjectsLib;
using QSReport;
using RevenueService.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TrueMark.Codes.Pool;
using TrueMarkApi.Client;
using Vodovoz.Additions;
using Vodovoz.Additions.Printing;
using Vodovoz.Application;
using Vodovoz.Application.Logistics;
using Vodovoz.Application.Logistics.Fuel;
using Vodovoz.Application.Mango;
using Vodovoz.Application.Pacs;
using Vodovoz.CachingRepositories.Cash;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.CachingRepositories.Counterparty;
using Vodovoz.Commons;
using Vodovoz.Core;
using Vodovoz.Core.Application.Entity;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Repositories.Logistics;
using Vodovoz.Core.Domain.Interfaces.Logistics;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Data.NHibernate;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Sms;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Suppliers;
using Vodovoz.Infrastructure.FileStorage;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Models;
using Vodovoz.Models.TrueMark;
using Vodovoz.Options;
using Vodovoz.PermissionExtensions;
using Vodovoz.Presentation.Reports.Factories;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Presentation.ViewModels.Controls.EntitySelection;
using Vodovoz.Presentation.ViewModels.Mango;
using Vodovoz.Presentation.ViewModels.Pacs;
using Vodovoz.Presentation.ViewModels.PaymentTypes;
using Vodovoz.Presentation.Views;
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
using Vodovoz.Services.Fuel;
using Vodovoz.Services.Logistics;
using Vodovoz.Services.Permissions;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Counterparty;
using Vodovoz.SidePanel.InfoViews;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Interactive.ConfirmationQuestion;
using Vodovoz.Tools.Interactive.YesNoCancelQuestion;
using Vodovoz.Tools.Logistic;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Dialogs.Mango;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Infrastructure;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Infrastructure.Services.Fuel;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Mango;
using Vodovoz.ViewModels.Permissions;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewWidgets;
using VodovozInfrastructure;
using VodovozInfrastructure.Endpoints;
using VodovozInfrastructure.Interfaces;
using VodovozInfrastructure.Services;
using VodovozInfrastructure.StringHandlers;
using static Vodovoz.ViewModels.Cash.Reports.CashFlowAnalysisViewModel;
using DocumentPrinter = Vodovoz.Core.DocumentPrinter;
using IErrorReporter = Vodovoz.Tools.IErrorReporter;

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddWaterDeliveryDesktop(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddVodovozViewModels()
				.AddPresentationViews()
				.AddDocumentPrinter()
				.AddTrueMarkApiClient()
				.AddSingleton<Startup>()
				.AddSingleton<IDatabaseConnectionSettings>((provider) =>
				{
					//Необходимо поменять логику работы окна логина,
					//чтобы правильно возвращать данные для подключения не используя статические классы
					var builder = QSMain.ConnectionStringBuilder;
					return new DatabaseConnectionSettings
					{
						ServerName = builder.Server,
						Port = builder.Port,
						DatabaseName = builder.Database,
						UserName = builder.UserID,
						Password = builder.Password,
						MySqlSslMode = builder.SslMode
					};
				})
				.AddSingleton<MySqlConnectionStringBuilder>(provider =>
				{
					var connectionSettings = provider.GetRequiredService<IDatabaseConnectionSettings>();
					var builder = new MySqlConnectionStringBuilder
					{
						Server = connectionSettings.ServerName,
						Port = connectionSettings.Port,
						Database = connectionSettings.DatabaseName,
						UserID = connectionSettings.UserName,
						Password = connectionSettings.Password,
						SslMode = connectionSettings.MySqlSslMode
					};

					if(connectionSettings.DefaultCommandTimeout.HasValue)
					{
						builder.DefaultCommandTimeout = connectionSettings.DefaultCommandTimeout.Value;
					}

					builder.Add("ConnectionTimeout", 120);

					return builder;

				})
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(QS.Project.HibernateMapping.TypeOfEntityMap).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(QS.Report.Domain.UserPrintSettings).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly
				)
				.AddDatabaseConfigurationExposer(config =>
				{
					config.DataBaseIntegration(
						dbi =>
						{
							dbi.BatchSize = 100;
							dbi.Timeout = 120;
						}
					);

					config.LinqToHqlGeneratorsRegistry<LinqToHqlGeneratorsRegistry>();
				})
				.ConfigureOptions<ConfigureS3Options>()
				.AddCoreDataNHibernate()
				.AddSpatialSqlConfiguration()
				.AddNHibernateConfiguration()
				.AddNHibernateConventions()
				.AddDatabaseInfo()
				.AddDatabaseSingletonSettings()
				.AddCore()
				.AddDesktop()
				.AddFileStorage()
				.AddGuiTrackedUoW()
				.AddObjectValidatorWithGui()
				.AddPermissionValidation()
				.AddGuiInteracive()
				.AddSlaveDbPreferredReportsCore()

				.AddScoped<IScanDialogService, ScanDialogService>()

				.AddScoped<IRouteListService, RouteListService>()
				.AddScoped<RouteGeometryCalculator>()
				.AddSingleton<OsrmClient>(sp => OsrmClientFactory.Instance)

				.AddScoped<IDebtorsSettings, DebtorsSettings>()
				.AddFiasClient()
				.AddScoped<RevisionBottlesAndDeposits>()
				.AddTransient<IReportExporter, ReportExporterAdapter>()
				.AddScoped<SelectPaymentTypeViewModel>()
				.AddScoped<ICoordinatesParser, CoordinatesParser>()
				.AddScoped<ICustomReportFactory, CustomReportFactory>()
				.AddScoped<ICustomPropertiesFactory, CustomPropertiesFactory>()
				.AddScoped<ICustomReportItemFactory, CustomReportItemFactory>()
				.AddScoped<IDriverWarehouseEventRepository, DriverWarehouseEventRepository>()
				.AddScoped<ICompletedDriverWarehouseEventProxyRepository, CompletedDriverWarehouseEventProxyRepository>()
				.AddScoped<IRdlTextBoxFactory, RdlTextBoxFactory>()
				.AddScoped<IEventsQrPlacer, EventsQrPlacer>()
				.AddTransient<IValidationViewFactory, GtkValidationViewFactory>()
				.AddSingleton<ViewModelWidgetResolver, BasedOnNameViewModelWidgetResolver>()
				.AddSingleton<ITDIWidgetResolver>(sp => sp.GetService<ViewModelWidgetResolver>())
				.AddSingleton<IFilterWidgetResolver>(sp => sp.GetService<ViewModelWidgetResolver>())
				.AddSingleton<IWidgetResolver>(sp => sp.GetService<ViewModelWidgetResolver>())
				.AddSingleton<IGtkViewResolver>(sp => sp.GetService<ViewModelWidgetResolver>())
				.AddSingleton<ViewModelWidgetsRegistrar>()
				.AddApplication()
				.AddBusiness(configuration)
				.AddDriverApiNotificationsSenders()
				.AddInfrastructure()
				.AddCoreDataRepositories()
				.AddScoped<IFuelApiService, FuelApiService>()
				.AddScoped<IFuelCardVersionService, FuelCardVersionService>()
				.AddFuelControl()
				.AddCodesPool()

				//Messages
				.AddSingleton<MessagesHostedService>()
				.AddSingleton<IMessageTransportInitializer>(ctx => ctx.GetRequiredService<MessagesHostedService>())
				.AddHostedService(ctx => ctx.GetRequiredService<MessagesHostedService>())

				.AddTransient(typeof(ViewModelEEVMBuilder<>))
				.AddTransient(typeof(LegacyEntitySelectionViewModelBuilder<>))
				.AddTransient<EntityModelFactory>()

				.AddPacs()
				.AddScoped<MessageService>();

			services.AddStaticHistoryTracker();
			services.AddStaticScopeForEntity();
			services.AddStaticServicesConfig();

			services.AddSingleton<IApplicationInfo, ApplicationVersionInfo>();

			services.AddSingleton<IErrorReportingSettings>(_ => new ErrorReportingSettings(false, false, true, 100));

			services.AddSingleton<ILogService, LogService>();

			#region Репозитории

			services.AddSingleton<IUserPrintingRepository, UserPrintingRepository>();

			#endregion

			#region Сервисы

			//GtkUI

			services.AddScoped<IConfirmationQuestionInteractive, GtkConfirmationQuestionInteractive>();
			services.AddScoped<IYesNoCancelQuestionInteractive, GtkYesNoCancelQuestionInteractive>();

			services.AddScoped<ICommonServices>(_ => ServicesConfig.CommonServices);
			services.AddScoped<IDeleteEntityService, DeleteEntityGUIService>();

			services.AddScoped<DeleteConfiguration>(_ => DeleteConfig.Main);

			services.AddScoped<ICurrentPermissionService>(_ => PermissionsSettings.CurrentPermissionService);

			services.AddScoped<IReportPrinter, ReportPrinter>();

			services.AddScoped<ICustomPrintRdlDocumentsPrinter, CustomPrintRdlDocumentsPrinter>();

			services.AddScoped<IEntityDeleteWorker, EntityDeleteWorker>();

			services.AddScoped<CommonMessages>();

			#endregion

			#region Старые общие диалоги

			services.AddScoped<ReportViewDlg>();

			#endregion

			#region Навигация

			services.AddScoped<IPageHashGenerator, ClassNamesHashGenerator>();

			services.AddScoped<IViewModelsPageFactory>(sp => new AutofacViewModelsTdiPageFactory(sp.GetRequiredService<ILifetimeScope>()));
			services.AddScoped<ITdiPageFactory>(sp => new AutofacTdiPageFactory(sp.GetRequiredService<ILifetimeScope>()));
			services.AddScoped<AutofacViewModelsGtkPageFactory>(sp => new AutofacViewModelsGtkPageFactory(sp.GetRequiredService<ILifetimeScope>()));
			services.AddScoped<TdiNotebook>(sp => TDIMain.MainNotebook);

			services.AddScoped<TdiNavigationManagerAdapter>();
			services.AddScoped<INavigationManager>(sp => sp.GetRequiredService<TdiNavigationManagerAdapter>());
			services.AddSingleton<ITdiCompatibilityNavigation>(sp => sp.GetRequiredService<TdiNavigationManagerAdapter>());

			#endregion

			#region ViewModels

			services.AddScoped<IViewModelResolver>(sp => new AutofacViewModelResolver(sp.GetRequiredService<ILifetimeScope>()));
			services.AddScoped<IEntityChangeWatcher>(_ => NotifyConfiguration.Instance);

			Assembly.GetExecutingAssembly()
				.GetTypes()
				.Concat(Assembly.GetAssembly(typeof(ComplaintViewModel)).GetTypes())
				.Concat(Assembly.GetAssembly(typeof(PacsPanelViewModel)).GetTypes())
				.Where(t =>
					t.IsClass
					&& !t.IsAbstract
					&& t.IsAssignableTo<ViewModelBase>()
					&& t.Name.EndsWith("ViewModel"))
				.ToList()
				.ForEach(t => services.AddScoped(t));

			services.AddScoped<PrepareDeletionViewModel>();
			services.AddScoped<DeletionProcessViewModel>();

			services.AddScoped<DeletionViewModel>();
			services.AddScoped<RdlViewerViewModel>();
			services.AddScoped<ProgressWindowViewModel>();
			services.AddScoped<IPacsViewModelFactory, PacsViewModelFactory>();

			#endregion

			#region Обработчики ошибок

			services.AddScoped<MySqlException1055OnlyFullGroupBy>();
			services.AddScoped<MySqlException1366IncorrectStringValue>();
			services.AddScoped<NHibernateFlushAfterException>();

			#endregion Обработчики ошибок

			// Классы водовоза

			services.AddScoped<WaterFixedPricesGenerator>();

			services.AddScoped<IRouteListDailyNumberProvider, RouteListDailyNumberProvider>();

			#region TrueMark

			services.AddScoped<Models.TrueMark.TrueMarkCodesPool>();

			services.AddScoped<TrueMarkCodePoolLoader>();

			services.AddScoped<TrueMarkCodesChecker>();

			services.AddScoped<ITrueMarkCodesValidator, TrueMarkTaskCodesValidator>();

			services.AddScoped<TrueMarkWaterCodeParser>();

			#endregion TrueMark

			services.AddScoped<ReceiptManualController>();

			services.AddScoped<FiscalizationResultSaver>();

			services.AddCashReceiptClientChannel();

			services.AddScoped<IOperatorStateMachine, OperatorStateMachine>();
			services.AddScoped<IOperatorClientFactory, OperatorClientFactory>();
			services.AddScoped<IOperatorClient, OperatorClient>();

			services.AddScoped<PacsDashboardModel>();
			services.AddScoped<IObserver<OperatorState>>(sp => sp.GetRequiredService<PacsDashboardModel>());
			services.AddScoped<IObserver<PacsCallEvent>>(sp => sp.GetRequiredService<PacsDashboardModel>());

			services.AddSingleton<IPacsDashboardViewModelFactory, PacsDashboardViewModelFactory>();

			services.AddScoped<PacsEmployeeProvider>();
			services.AddScoped<IPacsEmployeeProvider>(sp => sp.GetRequiredService<PacsEmployeeProvider>());
			services.AddScoped<IPacsOperatorProvider>(sp => sp.GetRequiredService<PacsEmployeeProvider>());
			services.AddScoped<IPacsAdministratorProvider>(sp => sp.GetRequiredService<PacsEmployeeProvider>());

			services.AddScoped<IFileChooserProvider, FileChooser>();
			services.AddScoped<ISmsNotifier, SmsNotifier>();

			#region Adapters & Factories

			services.AddScoped<IGtkTabsOpener, GtkTabsOpener>();
			services.AddScoped<IRDLPreviewOpener, RdlPreviewOpener>();
			services.AddSingleton<IReportViewOpener, GtkReportViewOpener>();
			services.AddScoped<RoboatsJournalsFactory>();

			Assembly.GetExecutingAssembly()
				.GetTypes()
				.Concat(Assembly.GetAssembly(typeof(VodovozViewModelAssemblyFinder)).GetTypes())
				.Concat(Assembly.GetAssembly(typeof(Vodovoz.Presentation.ViewModels.AssemblyFinder)).GetTypes())
				.Where(t => t.IsClass
					&& !t.IsAbstract 
					&& t.Name.EndsWith("Factory")
					&& t.GetInterfaces()
						.Where(i => i.Name == $"I{t.Name}")
						.FirstOrDefault() != null)
				.ToList()
				.ForEach(t =>
				{
					if(t.GetInterfaces().FirstOrDefault(i => i.Name == $"I{t.Name}") is Type @interface)
					{
						services.AddScoped(@interface, t);
					}
				});

			services.AddScoped<IIncludeExcludeSalesFilterFactory, IncludeExcludeSalesFilterFactory>();
			services.AddScoped<ILeftRightListViewModelFactory, LeftRightListViewModelFactory>();
			services.AddScoped<IPacsViewModelOpener, PacsViewModelOpener>();

			#endregion

			#region Controllers

			services.AddSingleton<GeoGroupVersionsModel>();
			services.AddScoped<IStringHandler, StringHandler>();

			#endregion

			#region Services

			services.AddScoped<IEmployeeService, EmployeeService>();
			services.AddScoped<IFileDialogService, FileDialogService>();
			services.AddScoped<IErrorReporter>(c => ErrorReporter.Instance);
			services.AddScoped<IWarehousePermissionService, WarehousePermissionService>();
			services.AddScoped<UsersPresetPermissionValuesGetter>();
			services.AddScoped<UsersEntityPermissionValuesGetter>();
			services.AddScoped<UserPermissionsExporter>();
			services.AddScoped<IAuthorizationService, AuthorizationService>();
			services.AddScoped<IUserSettingsService, UserSettingsService>();
			services.AddScoped<StoreDocumentHelper>();
			services.AddScoped<IWageParameterService, WageParameterService>();
			services.AddScoped<ISelfDeliveryCashOrganisationDistributor, SelfDeliveryCashOrganisationDistributor>();
			services.AddScoped<IEdoService, EdoService.Library.EdoService>();
			services.AddScoped<IEmailService, EmailService>();

			#endregion

			#region Models

			Assembly.GetAssembly(typeof(VodovozBusinessAssemblyFinder))
				.GetTypes()
				.Where(t => t.IsClass
					&& !t.IsAbstract
					&& t.Name.EndsWith("Model")
					&& !t.Name.EndsWith("ViewModel")
					&& t.GetInterfaces()
						.Where(i => i.Name == $"I{t.Name}")
						.FirstOrDefault() != null)
				.ForEach(t =>
				{
					if(t.GetInterfaces().FirstOrDefault(i => i.Name == $"I{t.Name}") is Type @interface)
					{
						services.AddScoped(@interface, t);
					}
				});

			services.AddScoped<WageParameterService>();

			services.AddScoped<UserWarehousePermissionModel>();
			services.AddScoped<WarehousePermissionModelBase>(sp => sp.GetRequiredService<UserWarehousePermissionModel>());

			#endregion

			#region CallTasks

			services.AddScoped<ICallTaskFactory>(_ => CallTaskSingletonFactory.GetInstance());

			services.AddScoped<ICallTaskWorker, CallTaskWorker>();

			#endregion

			#region Кэширующие репозитории

			services.AddScoped<IDomainEntityNodeInMemoryCacheRepository<FinancialExpenseCategory>, FinancialExpenseCategoriesNodesInMemoryCacheRepository>();
			services.AddScoped<IDomainEntityNodeInMemoryCacheRepository<FinancialIncomeCategory>, FinancialIncomeCategoriesNodesInMemoryCacheRepository>();
			services.AddScoped<IDomainEntityNodeInMemoryCacheRepository<Counterparty>, CounterpartyInMemoryTitlesCacheRepository>();

			#endregion Кэширующие репозитории

			#region Mango


			services.AddSingleton<IMangoViewModelNavigator, MangoViewModelNavigator>();

			services.AddSingleton<MangoManager>();
			services.AddSingleton<IMangoManager>(sp => sp.GetRequiredService<MangoManager>());

			#endregion

			#region Reports

			services.AddScoped<CashFlow>();
			services.AddScoped<WayBillReportGroupPrint>();
			services.AddScoped<StockMovements>();
			services.AddScoped<SalaryRatesReport>();
			services.AddScoped<AnalyticsForUndeliveryReport>();
			services.AddScoped<PaymentsFromAvangardReport>();
			services.AddScoped<EmployeesTaxesSumReport>();
			services.AddScoped<EmployeesFines>();
			services.AddScoped<SalesReportView>();
			services.AddScoped<SalesByDiscountReport>();
			services.AddScoped<DriverWagesReport>();
			services.AddScoped<FuelReport>();
			services.AddScoped<ShortfallBattlesReport>();
			services.AddScoped<WagesOperationsReport>();
			services.AddScoped<EquipmentReport>();
			services.AddScoped<ForwarderWageReport>();
			services.AddScoped<CashierCommentsReport>();
			services.AddScoped<OnecCommentsReport>();
			services.AddScoped<DriversWageBalanceReport>();
			services.AddScoped<QualityReport>();
			services.AddScoped<DriverRoutesListRegisterReport>();
			services.AddScoped<RoutesListRegisterReport>();
			services.AddScoped<OrdersByDistrictReport>();
			services.AddScoped<CompanyTrucksReport>();
			services.AddScoped<LastOrderByDeliveryPointReport>();
			services.AddScoped<OrderIncorrectPrices>();
			services.AddScoped<OrdersWithMinPriceLessThan>();
			services.AddScoped<RouteListsOnClosingReport>();
			services.AddScoped<OnLoadTimeAtDayReport>();
			services.AddScoped<SelfDeliveryReport>();
			services.AddScoped<ShipmentReport>();
			services.AddScoped<BottlesMovementReport>();
			services.AddScoped<MileageReport>();
			services.AddScoped<MastersReport>();
			services.AddScoped<SuburbWaterPriceReport>();
			services.AddScoped<BottlesMovementSummaryReport>();
			services.AddScoped<DrivingCallReport>();
			services.AddScoped<MastersVisitReport>();
			services.AddScoped<NotDeliveredOrdersReport>();
			services.AddScoped<EmployeesPremiums>();
			services.AddScoped<OrderStatisticByWeekReport>();
			services.AddScoped<ReportForBigClient>();
			services.AddScoped<OrderRegistryReport>();
			services.AddScoped<EquipmentBalance>();
			services.AddScoped<CardPaymentsOrdersReport>();
			services.AddScoped<DefectiveItemsReport>();
			services.AddScoped<OrdersByDistrictsAndDeliverySchedulesReport>();
			services.AddScoped<OrdersByCreationDateReport>();
			services.AddScoped<NomenclatureForShipment>();
			services.AddScoped<NotFullyLoadedRouteListsReport>();
			services.AddScoped<FirstClientsReport>();
			services.AddScoped<TariffZoneDebts>();
			services.AddScoped<ClientsByDeliveryPointCategoryAndActivityKindsReport>();
			services.AddScoped<ExtraBottleReport>();
			services.AddScoped<FirstSecondClientReport>();
			services.AddScoped<FuelConsumptionReport>();
			services.AddScoped<CounterpartyCloseDeliveryReport>();
			services.AddScoped<IncomeBalanceReport>();
			services.AddScoped<CashBookReport>();
			services.AddScoped<ProfitabilityBottlesByStockReport>();
			services.AddScoped<PlanImplementationReport>();
			services.AddScoped<ZeroDebtClientReport>();
			services.AddScoped<OrdersCreationTimeReport>();
			services.AddScoped<PaymentsFromBankClientFinDepartmentReport>();
			services.AddScoped<ChainStoreDelayReport>();
			services.AddScoped<ReturnedTareReport>();
			services.AddScoped<ProductionRequestReport>();
			services.AddScoped<FuelConsumptionReport>();
			services.AddScoped<NonClosedRLByPeriodReport>();
			services.AddScoped<EShopSalesReport>();
			services.AddScoped<CounterpartyReport>();
			services.AddScoped<DriversToDistrictsAssignmentReport>();
			services.AddScoped<GeneralSalaryInfoReport>();
			services.AddScoped<AddressesOverpaymentsReport>();
			services.AddScoped<StockMovementsAdvancedReport>();

			#endregion

			#region Старые диалоги

			Assembly.GetAssembly(typeof(CounterpartyDlg))
				.GetTypes()
				.Where(t => t.IsClass
					&& !t.IsAbstract
					&& t.IsAssignableTo<ITdiTab>())
				.ToList()
				.ForEach(t => services.AddScoped(t));

			#endregion

			#region Фильтры

			services.AddScoped<PaymentsJournalFilterViewModel>();
			services.AddScoped<UnallocatedBalancesJournalFilterViewModel>();
			services.AddScoped<SelectableParametersReportFilter>();
			services.AddScoped<RequestsToSuppliersFilterViewModel>();

			#endregion

			#region Классы

			services.AddScoped<IncludeExludeFiltersViewModel>();

			services.AddScoped<User>();
			services.AddScoped<EntitySubdivisionForUserPermission>();
			services.AddScoped<EntityUserPermissionExtended>();
			services.AddScoped<EntityUserPermission>();
			services.AddScoped<HierarchicalPresetUserPermission>();
			services.AddScoped<UserWarehousePermission>();
			services.AddScoped<EntityUserPermissionExtended>();

			services.AddScoped<IPermissionNode, UserPermissionNode>();

			services.AddScoped<DriverApiUserRegisterEndpoint>(sp =>
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
			});

			services.AddScoped<UserSettings>(c => CurrentUserSettings.Settings);

			services.AddScoped<IPasswordGenerator, PasswordGenerator>();

			services.AddScoped<IStoreDocumentHelper, StoreDocumentHelper>();

			services.AddScoped<IAdvanceCashOrganisationDistributor, AdvanceCashOrganisationDistributor>();

			services.AddScoped<IRouteListCashOrganisationDistributor, RouteListCashOrganisationDistributor>();

			services.AddScoped<IIncomeCashOrganisationDistributor, IncomeCashOrganisationDistributor>();

			services.AddScoped<IExpenseCashOrganisationDistributor, ExpenseCashOrganisationDistributor>();

			services.AddScoped<IFuelCashOrganisationDistributor, FuelCashOrganisationDistributor>();

			services.AddScoped<IStoreDocumentHelper, StoreDocumentHelper>();

			services.AddScoped<CashFlowDdsReportRenderer>();

			services.AddScoped<IRevenueServiceClient>(sp =>
			{
				var counterpartySettings = sp.GetRequiredService<ICounterpartySettings>();

				return new RevenueServiceClient(counterpartySettings.RevenueServiceClientAccessToken);
			});

			#endregion

			#region InfoPanelViews

			services.AddScoped<CarsMonitoringInfoPanelView>();

			#endregion


			return services;
		}

		public static IServiceCollection AddPermissionValidation(this IServiceCollection services)
		{
			services.AddSingleton<IPermissionService, PermissionService>()
				.AddSingleton<IEntityExtendedPermissionValidator, EntityExtendedPermissionValidator>()
				.AddSingleton<IWarehousePermissionValidator, WarehousePermissionValidator>()
				.AddSingleton<IPermissionExtensionStore>(sp => PermissionExtensionSingletonStore.GetInstance())
				.AddScoped<IDocumentPrinter, DocumentPrinter>();

			services.AddSingleton<IEntityPermissionValidator, Vodovoz.Domain.Permissions.EntityPermissionValidator>()
				.AddSingleton<IPresetPermissionValidator, Vodovoz.Domain.Permissions.HierarchicalPresetPermissionValidator>();

			return services;
		}

		public static IServiceCollection AddDocumentPrinter(this IServiceCollection services) =>
			services.AddScoped<IDocumentPrinter, DocumentPrinter>();

		public static IServiceCollection AddPacs(this IServiceCollection services)
		{
			services.AddPacsOperatorClient()
				.AddSingleton<SettingsConsumer>()
				.AddSingleton<IObservable<SettingsEvent>>(ctx => ctx.GetRequiredService<SettingsConsumer>())
				.AddScoped<PacsEndpointsConnector>()
				.AddScoped<MessageEndpointConnector>()
				.AddSingleton<OperatorStateAdminConsumer>()
				.AddSingleton<IObservable<OperatorState>>(ctx => ctx.GetRequiredService<OperatorStateAdminConsumer>());

			services.AddHttpClient<IAdminClient, AdminClient>(c =>
			{
				c.DefaultRequestHeaders.Clear();
				c.DefaultRequestHeaders.Add("Accept", "application/json");
			});

			services.AddPacsMassTransitNotHosted(
				(context, rabbitCfg) =>
				{
					rabbitCfg.AddPacsBaseTopology(context);
					rabbitCfg.AddEdoTopology(context);
				},
				(busCfg) =>
				{
					//Оператор
					busCfg.AddConsumer<OperatorStateConsumer, OperatorStateConsumerDefinition>();
					busCfg.AddConsumer<OperatorsOnBreakConsumer, OperatorsOnBreakConsumerDefinition>();
					busCfg.AddConsumer<OperatorSettingsConsumer, OperatorSettingsConsumerDefinition>();

					//Админ
					busCfg.AddConsumer<OperatorStateAdminConsumer, OperatorStateAdminConsumerDefinition>();
					busCfg.AddConsumer<SettingsConsumer, SettingsConsumerDefinition>();
					busCfg.AddConsumer<PacsCallEventConsumer, PacsCallEventConsumerDefinition>();
				}
				//Exclude необходим для отложенного запуска конечной точки, или отмены запуска по условию
				//При этом добавление определения потребителя в конфигурации обязательно
				, (filter) =>
				{
					filter.Exclude<SettingsConsumer>();
					filter.Exclude<OperatorSettingsConsumer>();
					filter.Exclude<OperatorStateAdminConsumer>();
					filter.Exclude<OperatorStateConsumer>();
					filter.Exclude<OperatorsOnBreakConsumer>();
					filter.Exclude<PacsCallEventConsumer>();
				}
			);

			return services;
		}
	}
}
