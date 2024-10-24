using Autofac;
using CashReceiptApi.Authentication;
using CashReceiptApi.HealthChecks;
using CashReceiptApi.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using QS.Project.Core;
using System.Configuration;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;
using Vodovoz.Settings.Database;
using Vodovoz.Tools;
using VodovozHealthCheck;

namespace CashReceiptApi
{
	public class Startup
	{
		private const string _nLogSectionName = nameof(NLog);

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
				})
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(EmployeeWithLoginMap).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()
				.AddDatabaseSettings()
				.AddInfrastructure()
				;

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);

			services.ConfigureHealthCheckService<CashReceiptApiHealthChecks>();
			services.Configure<ServiceOptions>(Configuration.GetSection(nameof(ServiceOptions)));
			services.AddAuthentication()
				.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, null);
			services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme);
			services.AddGrpc().Services.AddAuthorization();
			services.AddMvc().AddControllersAsServices();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterType<ApiKeyAuthenticationOptions>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<ApiKeyAuthenticationHandler>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<CashboxClientProvider>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<FiscalDocumentRefresher>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<FiscalDocumentRequeueService>()
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

			app.ConfigureHealthCheckApplicationBuilder();
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
