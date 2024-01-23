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
using System.Configuration;
using System.Reflection;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;

namespace CashReceiptSendWorker
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
			var nlogConfig = Configuration.GetSection(_nLogSectionName);
			services.AddLogging(
				logging =>
				{
					logging.ClearProviders();
					logging.AddNLogWeb();
					logging.AddConfiguration(nlogConfig);
				});

			services
				.AddCore()
				.AddTrackedUoW()
				;

			services.AddHostedService<CashReceiptSendWorker>();

			CreateBaseConfig(services);
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterModule<DatabaseSettingsModule>();

			builder.RegisterType<CashReceiptRepository>()
				.As<ICashReceiptRepository>()
				.InstancePerDependency();

			builder.RegisterType<CashReceiptsSender>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<FiscalDocumentPreparer>()
				.AsSelf()
				.SingleInstance();

			builder.RegisterType<StaleReceiptDocumentsRefresher>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<CashReceiptDistributor>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<CashboxClientProvider>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<FiscalDocumentRefresher>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<TrueMarkWaterCodeParser>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<CashboxClientFactory>()
				.WithParameter(TypedParameter.From(GetCashboxBaseUrl()))
				.WithProperty(x => x.IsTestMode, IsTestMode())
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.Register((context) => new CashboxSettingProvider(GetCashboxesConfiguration()))
				.As<ICashboxSettingProvider>()
				.SingleInstance();

			builder.RegisterType<FiscalizationResultSaver>()
				.AsSelf()
				.InstancePerLifetimeScope();

			//Убрать когда IOrderParametersProvider заменится на IOrderSettings, будет зарегистрирована как модуль DatabaseSettingsModule
			builder.RegisterType<OrderParametersProvider>()
				.As<IOrderParametersProvider>()
				.SingleInstance();

			//Убрать когда IOrderParametersProvider заменится на IOrderSettings, будет зарегистрирована как модуль DatabaseSettingsModule
			builder.RegisterType<ParametersProvider>()
				.As<IParametersProvider>()
				.SingleInstance();

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

			conStrBuilder.Server = domainDBConfig.GetValue<string>("server");
			conStrBuilder.Port = domainDBConfig.GetValue<uint>("port");
			conStrBuilder.Database = domainDBConfig.GetValue<string>("database");
			conStrBuilder.UserID = domainDBConfig.GetValue<string>("user");
			conStrBuilder.Password = domainDBConfig.GetValue<string>("password");
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

		private IConfigurationSection GetCashboxesConfiguration()
		{
			return Configuration.GetSection("Cashboxes");
		}

		private string GetCashboxBaseUrl()
		{
			var modulKassaSection = Configuration.GetSection("ModulKassa");
			if(!modulKassaSection.Exists())
			{
				throw new ConfigurationErrorsException("Не удается загрузить конфигурацию для модуль кассы.");
			}

			string baseUrlConfig = modulKassaSection["baseAddress"];
			if(string.IsNullOrWhiteSpace(baseUrlConfig))
			{
				throw new ConfigurationErrorsException("Не удается загрузить конфигурацию базового адреса api для модуль кассы.");
			}

			return baseUrlConfig;
		}

		private bool IsTestMode()
		{
			var modulKassaSection = Configuration.GetSection("ModulKassa");
			if(!modulKassaSection.Exists())
			{
				throw new ConfigurationErrorsException("Не удается загрузить конфигурацию для модуль кассы.");
			}

			bool isTestMode = modulKassaSection.GetValue<bool>("isTestsMode");
			return isTestMode;
		}
	}
}
