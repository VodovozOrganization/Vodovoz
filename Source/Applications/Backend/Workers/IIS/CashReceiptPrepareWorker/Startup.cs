using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using QS.Project.Core;
using TrueMarkApi.Library;
using Vodovoz.Core.Data.NHibernate;
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
					typeof(Vodovoz.Settings.Database.AssemblyFinder).Assembly
				)
				.AddDatabaseConnection()
				.AddCore()
				.AddTrackedUoW()
				;

			services.AddHostedService<ReceiptsPrepareWorker>();
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
	}
}
