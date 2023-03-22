﻿using Autofac;
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
using System;
using System.Configuration;
using System.Reflection;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Models.CashReceipts;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;

namespace CashReceiptSendWorker
{
	public class Startup
	{
		private const string _nLogSectionName = "NLog";
		//private bool _testMode;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			//SetupMode();
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

			services.AddHostedService<CashReceiptSendWorker>();

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

			builder.RegisterType<CashReceiptRepository>()
				.As<ICashReceiptRepository>()
				.InstancePerDependency();

			builder.RegisterType<CashReceiptsSender>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<FiscalDocumentPreparer>()
				.AsSelf()
				.SingleInstance();

			builder.RegisterType<CashReceiptDistributor>()
				.AsSelf()
				.InstancePerLifetimeScope();

			//Сделать регистрацию строки для этого типа
			builder.RegisterType<CashboxClientFactory>()
				.WithParameter(TypedParameter.From(GetCashboxBaseUrl()))
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.Register((context) => new CashboxSettingProvider(GetCashboxesConfiguration()))
				.As<ICashboxSettingProvider>()
				.SingleInstance();

			//Убрать когда IOrderParametersProvider заменится на IOrderSettings, будет зарегистрирована как модуль DatabaseSettingsModule
			/*builder.RegisterType<OrderParametersProvider>()
				.As<IOrderParametersProvider>()
				.SingleInstance();*/

			//Убрать когда IOrderParametersProvider заменится на IOrderSettings, будет зарегистрирована как модуль DatabaseSettingsModule
			/*builder.RegisterType<ParametersProvider>()
				.As<IParametersProvider>()
				.SingleInstance();*/

			builder.RegisterInstance(ErrorReporter.Instance)
				.As<IErrorReporter>()
				.SingleInstance();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
		}

		/*private void SetupMode()
		{
			var commonConfig = Configuration.GetSection("Common");

			var modeValue = commonConfig["Mode"];
			switch(modeValue)
			{
				case "Test":
					_testMode = true;
					break;
				case "Work":
					return;
				default:
					throw new NotSupportedException($"Не поддерживаемый режим работы ({modeValue}) приложения");
			}
		}*/

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

			string baseUrlConfig = modulKassaSection["base_address"];
			if(string.IsNullOrWhiteSpace(baseUrlConfig))
			{
				throw new ConfigurationErrorsException("Не удается загрузить конфигурацию базового адреса api для модуль кассы.");
			}

			return baseUrlConfig;
		}
	}
}
