using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using QS.Project.Core;
using TrueMarkApi.Client;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Factories;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;
using Vodovoz.Tools;
using VodovozBusiness.Models.TrueMark;

namespace CashReceiptPrepareWorker
{
	public class Startup
	{
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
					logging.AddConfiguration(Configuration.GetSection("NLog"));
				});

			services
				.AddMappingAssemblies(
					typeof(QS.Project.HibernateMapping.UserBaseMap).Assembly,
					typeof(Vodovoz.Data.NHibernate.AssemblyFinder).Assembly,
					typeof(QS.Banks.Domain.Bank).Assembly,
					typeof(QS.HistoryLog.HistoryMain).Assembly,
					typeof(QS.Project.Domain.TypeOfEntity).Assembly,
					typeof(QS.Attachments.Domain.Attachment).Assembly,
					typeof(EmployeeWithLoginMap).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddInfrastructure()
				.AddTrackedUoW()
				.AddHttpClient()
			;

			Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
			services.AddHostedService<ReceiptsPrepareWorker>();
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterType<CashReceiptFactory>()
				.As<ICashReceiptFactory>()
				.SingleInstance();

			builder.RegisterType<ReceiptPreparerFactory>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<OrderReceiptCreatorFactory>()
				.AsSelf()
				.InstancePerLifetimeScope();

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

			builder.RegisterType<OurCodesChecker>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterInstance(ErrorReporter.Instance)
				.As<IErrorReporter>()
				.SingleInstance();

			builder.RegisterType<Tag1260Checker>()
				.As<ITag1260Checker>();

			builder.Register((context) => new ModulKassaOrganizationSettingProvider(GetModulKassaOrganizationsSetting()))
				.As<IModulKassaOrganizationSettingProvider>()
				.SingleInstance();

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
		}

		private IConfigurationSection GetModulKassaOrganizationsSetting()
		{
			return Configuration.GetSection("ModulKassaOrganizationSettings");
		}
	}
}
