﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using FastPaymentsAPI.Library.ApiClients;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Extensions.Logging;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using System.Reflection;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.FastPayments;
using Vodovoz.Tools;

namespace FastPaymentsNotificationWorker
{
	public class Program
	{
		private const string _nLogSectionName = nameof(NLog);

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory(ConfigureContainer))
				.UseWindowsService()
				.ConfigureServices((context, services) =>
				{
					services.AddLogging(
						logging =>
						{
							logging.ClearProviders();
							logging.AddNLog();
							logging.AddConfiguration(context.Configuration.GetSection(_nLogSectionName));
						});

					services.AddHostedService<PaymentsNotificationWorker>();

					CreateBaseConfig(context.Configuration);
				});

		public static void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterModule<DatabaseSettingsModule>();

			builder.RegisterType<DefaultSessionProvider>()
				.As<ISessionProvider>()
				.SingleInstance();

			builder.RegisterType<DefaultUnitOfWorkFactory>()
				.As<IUnitOfWorkFactory>()
				.SingleInstance();

			builder.RegisterType<NotificationHandler>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<FastPaymentRepository>()
				.As<IFastPaymentRepository>()
				.SingleInstance();

			builder.RegisterType<FastPaymentFactory>()
				.As<IFastPaymentFactory>()
				.InstancePerLifetimeScope();

			builder.RegisterType<SiteSettings>()
				.As<ISiteSettings>()
				.InstancePerLifetimeScope();

			builder.RegisterType<OrderSumConverter>()
				.As<IOrderSumConverter>()
			.InstancePerLifetimeScope();

			builder.RegisterType<SiteClient>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<SiteNotifier>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<MobileAppNotifier>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<MobileAppClient>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<NotificationModel>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterInstance(ErrorReporter.Instance)
				.As<IErrorReporter>()
				.SingleInstance();
		}

		private static void CreateBaseConfig(IConfiguration configuration)
		{
			var conStrBuilder = new MySqlConnectionStringBuilder();

			var domainDBConfig = configuration.GetSection("DomainDB");

			conStrBuilder.Server = domainDBConfig.GetValue<string>("Server");
			conStrBuilder.Port = domainDBConfig.GetValue<uint>("Port");
			conStrBuilder.Database = domainDBConfig.GetValue<string>("Database");
			conStrBuilder.UserID = domainDBConfig.GetValue<string>("UserID");
			conStrBuilder.Password = domainDBConfig.GetValue<string>("Password");
			conStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = conStrBuilder.GetConnectionString(true);

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString)
				.AdoNetBatchSize(100)
				.Driver<LoggedMySqlClientDriver>()
				;

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				db_config,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder)),
					typeof(DriverWarehouseEventMap).Assembly
				}
			);
		}
	}
}
