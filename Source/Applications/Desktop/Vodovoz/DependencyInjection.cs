using Core.Infrastructure;
using DriverApi.Notifications.Client;
using Edo.Transport;
using ExportTo1c.Library.Factories;
using Fias.Client;
using FuelControl.Library;
using Mailganer.Api.Client;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.Entity.EntityPermissions.EntityExtendedPermission;
using QS.DomainModel.Entity.PresetPermissions;
using QS.HistoryLog;
using QS.Osrm;
using QS.Project;
using QS.Project.Core;
using QS.Project.DB;
using QS.Project.GtkSharp;
using QS.Services;
using QS.Tdi;
using QS.Validation;
using QS.ViewModels.Control.EEVM;
using QS.Views.Resolve;
using QSAttachment;
using QSProjectsLib;
using ResourceLocker.Library;
using System;
using TrueMark.Codes.Pool;
using TrueMarkApi.Client;
using Vodovoz.Additions;
using Vodovoz.Application;
using Vodovoz.Application.Logistics;
using Vodovoz.Application.Logistics.Fuel;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Commons;
using Vodovoz.Core;
using Vodovoz.Core.Application.Entity;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Repositories.Logistics;
using Vodovoz.Core.Domain.Interfaces.Logistics;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Data.NHibernate;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Factories;
using Vodovoz.Infrastructure.FileStorage;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Options;
using Vodovoz.PermissionExtensions;
using Vodovoz.Presentation.Reports.Factories;
using Vodovoz.Presentation.ViewModels.Controls.EntitySelection;
using Vodovoz.Presentation.ViewModels.PaymentTypes;
using Vodovoz.Presentation.Views;
using Vodovoz.Reports;
using Vodovoz.Services.Fuel;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Database.Counterparty;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels;
using Vodovoz.ViewModels.Infrastructure;
using Vodovoz.ViewModels.Infrastructure.Services;
using Vodovoz.ViewModels.Infrastructure.Services.Fuel;
using Vodovoz.ViewModels.Journals.Mappings;
using Vodovoz.ViewModels.Services;
using Vodovoz.ViewModels.TempAdapters;
using VodovozBusiness.Services.Orders;
using VodovozInfrastructure;
using VodovozInfrastructure.Services;
using DocumentPrinter = Vodovoz.Core.DocumentPrinter;
using Osrm;
using QS.Project.Journal;
using QS.Project.Repositories;
using Vodovoz.MainMenu;
using Vodovoz.MainMenu.AdministrationMenu;
using Vodovoz.MainMenu.BaseMenu;
using Vodovoz.MainMenu.HelpMenu;
using Vodovoz.MainMenu.JournalsMenu;
using Vodovoz.MainMenu.JournalsMenu.Accounting;
using Vodovoz.MainMenu.JournalsMenu.Banks;
using Vodovoz.MainMenu.JournalsMenu.Counterparties;
using Vodovoz.MainMenu.JournalsMenu.Financies;
using Vodovoz.MainMenu.JournalsMenu.Helpers;
using Vodovoz.MainMenu.JournalsMenu.Logistics;
using Vodovoz.MainMenu.JournalsMenu.Orders;
using Vodovoz.MainMenu.JournalsMenu.Organization;
using Vodovoz.MainMenu.JournalsMenu.Products;
using Vodovoz.MainMenu.JournalsMenu.Transports;
using Vodovoz.MainMenu.ProposalsMenu;
using Vodovoz.MainMenu.ReportsMenu;
using Vodovoz.MainMenu.ViewMenu;
using Vodovoz.ViewModels.ViewModels.Reports.Payments;
using RabbitMQ.MailSending;

namespace Vodovoz
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddWaterDeliveryDesktop(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddScoped<IClipboard, GtkClipboard>();

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

				.AddScoped<RouteGeometryCalculator>()
		
				.AddOsrm()

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
				.AddScoped<MessageService>()
				.AddSingleton<EntityToJournalMappings>()
				.AddScoped<EntityJournalOpener>()

				.AddMailganerApiClient()
				.AddScoped<EmailDirectSender>()
				
				.AddScoped<IDataExporterFor1cFactory, DataExporterFor1cFactory>()

				.AddVodovozDesktopResourceLocker()
				.AddScoped<BankAccountsMovementsJournalReport>()
				.AddMainMenuDependencies()
				.AddTransient(typeof(SimpleEntityJournalViewModel<,>))
				.AddScoped<IMySqlPasswordRepository, MySqlPasswordRepository>()
				.AddScoped<IPasswordValidator, PasswordValidator>()
				.AddScoped<IPasswordValidationSettings, DefaultPasswordValidationSettings>()
				;

			services.AddStaticHistoryTracker();
			services.AddStaticScopeForEntity();
			services.AddStaticServicesConfig();

			return services;
		}
		
		public static IServiceCollection AddMailganerApiClient(this IServiceCollection services)
		{
			services.AddOptions<MailganerSettings>().Configure<IConfiguration>((options, config) =>
			{
				config.GetSection("MailganerSettings").Bind(options);
			});

			services.AddTransient<MailganerClientV1>();
			services.AddTransient<MailganerClientV2>();

			services.AddHttpClient<MailganerClientV2>((sp, httpClient) =>
			{
				var settingsController = sp.GetRequiredService<ISettingsController>();
				var apiKey = settingsController.GetStringValue("MailganerSettings");

				httpClient.BaseAddress = new Uri("https://api.samotpravil.ru/api/v2/");
				httpClient.DefaultRequestHeaders.Clear();
				httpClient.DefaultRequestHeaders.Add("Authorization", $"{apiKey}");
			});

			services.AddHttpClient<MailganerClientV1>((sp, httpClient) =>
			{
				httpClient.BaseAddress = new Uri("https://api.samotpravil.ru/api/v1/");
			});

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
					rabbitCfg.AddSendEmailMessageTopology(context);
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

		public static IServiceCollection AddMainMenuDependencies(this IServiceCollection services) => services 
			.AddSingleton<MainMenuBarCreator>()
			.AddSingleton<ConcreteMenuItemCreator>()
			.AddSingleton<BaseMenuItemCreator>()
			.AddSingleton<ViewMenuItemCreator>()
			.AddSingleton<MainPanelMenuItemHandler>()
			.AddSingleton<TabsMenuItemHandler>()
			.AddSingleton<ThemesAppMenuItemHandler>()
			.AddSingleton<JournalsMenuItemCreator>()
			.AddSingleton<OrganizationMenuItemCreator>()
			.AddSingleton<WageMenuItemCreator>()
			.AddSingleton<ComplaintResultsMenuItemCreator>()
			.AddSingleton<ComplaintClassificationMenuItemCreator>()
			.AddSingleton<UndeliveryClassificationMenuItemCreator>()
			.AddSingleton<ProductsMenuItemCreator>()
			.AddSingleton<InventoryAccountingMenuItemCreator>()
			.AddSingleton<ExternalSourcesMenuItemCreator>()
			.AddSingleton<ExternalSourceCatalogsMenuItemCreator>()
			.AddSingleton<BanksMenuItemCreator>()
			.AddSingleton<FinancesMenuItemCreator>()
			.AddSingleton<CompanyBalanceMenuItemCreator>()
			.AddSingleton<CounterpartiesMenuItemCreator>()
			.AddSingleton<LogisticsMenuItemCreator>()
			.AddSingleton<DriverWarehouseEventsMenuItemCreator>()
			.AddSingleton<HelpersMenuItemCreator>()
			.AddSingleton<OrdersMenuItemCreator>()
			.AddSingleton<OnlineOrdersSourcesMenuItemCreator>()
			.AddSingleton<OrdersRatingsMenuItemCreator>()
			.AddSingleton<TransportMenuItemCreator>()
			.AddSingleton<AccountingMenuItemCreator>()
			.AddSingleton<ReportsMenuItemCreator>()
			.AddSingleton<OrderReportsMenuItemCreator>()
			.AddSingleton<SalesReportsMenuItemCreator>()
			.AddSingleton<WarehouseReportsMenuItemCreator>()
			.AddSingleton<OskOkkReportsMenuItemCreator>()
			.AddSingleton<LogisticReportsMenuItemCreator>()
			.AddSingleton<EmployeesReportsMenuItemCreator>()
			.AddSingleton<DriversReportsMenuItemCreator>()
			.AddSingleton<ServiceReportsMenuItemCreator>()
			.AddSingleton<AccountingDepReportsMenuItemCreator>()
			.AddSingleton<CashRegisterDepReportsMenuItemCreator>()
			.AddSingleton<ManufacturingReportsMenuItemCreator>()
			.AddSingleton<RetailReportsMenuItemCreator>()
			.AddSingleton<TransportReportsMenuItemCreator>()
			.AddSingleton<AdministrationMenuItemCreator>()
			.AddSingleton<AdminServiceMenuItemCreator>()
			.AddSingleton<HelpMenuItemCreator>()
			.AddSingleton<ProposalsMenuItemCreator>() 
		;
	}
}
