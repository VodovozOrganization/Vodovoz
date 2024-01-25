using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using QS.Project.Core;
using System.Configuration;
using Vodovoz.Core.Data.NHibernate;
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
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()
				;

			services.AddHostedService<CashReceiptSendWorker>();
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
