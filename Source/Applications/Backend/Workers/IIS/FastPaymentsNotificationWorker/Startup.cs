using Autofac;
using FastPaymentsAPI.Library.ApiClients;
using FastPaymentsAPI.Library.Converters;
using FastPaymentsAPI.Library.Factories;
using FastPaymentsAPI.Library.Notifications;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using System.Reflection;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.FastPayments;
using Vodovoz.Tools;

namespace FastPaymentsNotificationWorker
{
	public class Startup
	{
		private const string _nLogSectionName = "NLog";

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(Configuration.GetSection(_nLogSectionName));
				});

			services.AddHostedService<PaymentsNotificationWorker>();

			CreateBaseConfig();
		}

		public void ConfigureContainer(ContainerBuilder builder)
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

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
		}

		private void CreateBaseConfig()
		{
			var conStrBuilder = new MySqlConnectionStringBuilder();

			var domainDBConfig = Configuration.GetSection("DomainDB");

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
					Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.Organizations.OrganizationMap)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);
		}
	}
}
