using Autofac;
using CashReceiptApi.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Web;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Domain;
using System.Configuration;
using System.Reflection;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;

namespace CashReceiptApi
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

			services.AddAuthentication()
				.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, null);
			services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme);
			services.AddGrpc().Services.AddAuthorization();
			services.AddMvc().AddControllersAsServices();

			CreateBaseConfig();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterModule<DatabaseSettingsModule>();

			builder.RegisterType<ApiKeyAuthenticationOptions>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<ApiKeyAuthenticationHandler>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<DefaultSessionProvider>()
				.As<ISessionProvider>()
				.SingleInstance();

			builder.RegisterType<DefaultUnitOfWorkFactory>()
				.As<IUnitOfWorkFactory>()
				.SingleInstance();

			builder.RegisterType<CashboxClientProvider>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<FiscalDocumentRefresher>()
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
			if(env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseGrpcWeb();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGrpcService<CashReceiptService>().EnableGrpcWeb();
			});
		}

		private void CreateBaseConfig()
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
