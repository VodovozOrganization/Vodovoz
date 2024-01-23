using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.Core;
using QS.Project.DB;
using QS.Project.Domain;
using System.Reflection;
using TrueMarkApi.Library;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.TrueMark;
using Vodovoz.Factories;
using Vodovoz.Models.TrueMark;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;

namespace CashReceiptPrepareWorker
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

			services.AddCore()
				.AddTrackedUoW()
				;

			services.AddHostedService<ReceiptsPrepareWorker>();

			CreateBaseConfig(services);
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterModule<DatabaseSettingsModule>();

			builder.RegisterType<TrueMarkRepository>()
				.As<ITrueMarkRepository>()
				.SingleInstance();

			builder.RegisterType<OrderRepository>()
				.As<IOrderRepository>()
				.SingleInstance();
			
			builder.RegisterType<OrganizationRepository>()
				.As<IOrganizationRepository>()
				.SingleInstance();

			builder.RegisterType<CashReceiptFactory>()
				.As<ICashReceiptFactory>()
				.SingleInstance();

			//Убрать когда IOrderParametersProvider заменится на IOrderSettings, будет зарегистрирована как модуль DatabaseSettingsModule
			builder.RegisterType<OrderParametersProvider>()
				.As<IOrderParametersProvider>()
				.SingleInstance();

			//Убрать когда IOrderParametersProvider заменится на IOrderSettings, будет зарегистрирована как модуль DatabaseSettingsModule
			builder.RegisterType<ParametersProvider>()
				.As<IParametersProvider>()
				.SingleInstance();

			builder.RegisterType<ReceiptPreparerFactory>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<OrderReceiptCreatorFactory>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<CashReceiptRepository>()
				.As<ICashReceiptRepository>()
				.InstancePerDependency();

			builder.RegisterType<TrueMarkCodesChecker>()
				.AsSelf()
				.InstancePerDependency();

			builder.RegisterType<ReceiptsHandler>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<SelfdeliveryReceiptCreator>()
				.AsSelf()
				.InstancePerLifetimeScope();
			
			builder.RegisterType<DeliveryOrderReceiptCreator>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<TrueMarkApiClientFactory>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.Register<TrueMarkApiClient>((context, instance) => context.Resolve<TrueMarkApiClientFactory>().GetClient())
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<TrueMarkTransactionalCodesPool>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<TrueMarkWaterCodeParser>()
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

		private void CreateBaseConfig(IServiceCollection services)
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

			var provider = services.BuildServiceProvider();
			var ormConfig = provider.GetRequiredService<IOrmConfig>();
			ormConfig.ConfigureOrm(
				db_config,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(AssemblyFinder))
				}
			);
		}
	}
}
