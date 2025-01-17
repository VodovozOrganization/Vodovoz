using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using System.Threading.Tasks;
using TrueMarkApi.Client;
using TrueMarkCodesWorker;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;
using Vodovoz.Tools;
using VodovozBusiness.Models.TrueMark;

namespace TrueMarkCodePoolCheckWorker
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			await CreateHostBuilder(args).Build().RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
				{
					loggingBuilder.ClearProviders();
					loggingBuilder.AddNLog();
					loggingBuilder.AddConfiguration(hostBuilderContext.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory(ConfigureContainer))
				.ConfigureServices((hostContext, services) =>
				{
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

						.AddSingleton<IModulKassaOrganizationSettingProvider>((sp) =>
						{
							var configuration = sp.GetRequiredService<IConfiguration>();
							var modulKassaSettings = new ModulKassaOrganizationSettingProvider(configuration.GetSection("ModulKassaOrganizationSettings"));

							return modulKassaSettings;
						})

						.AddHostedService<CodePoolCheckWorker>()
						.AddHttpClient()
						;

					Vodovoz.Data.NHibernate.DependencyInjection.AddStaticScopeForEntity(services);
				});

		public static void ConfigureContainer(ContainerBuilder builder)
		{
			ErrorReporter.Instance.AutomaticallySendEnabled = false;
			ErrorReporter.Instance.SendedLogRowCount = 100;

			builder.RegisterType<TrueMarkCodesChecker>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<TrueMarkCodePoolChecker>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<OurCodesChecker>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<TrueMarkApiClientFactory>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.Register<TrueMarkApiClient>((context, instance) => context.Resolve<TrueMarkApiClientFactory>().GetClient())
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<TrueMarkCodesPool>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterType<TrueMarkWaterCodeParser>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterInstance(ErrorReporter.Instance)
				.As<IErrorReporter>()
				.SingleInstance();

			builder.RegisterType<Tag1260Checker>()
				.As<ITag1260Checker>();
		}
	}
}
