using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.HistoryLog;
using QS.Project.Core;
using QS.Project.Domain;
using QS.Project.HibernateMapping;
using System;
using System.Threading.Tasks;
using TrueMark.Codes.Pool;
using TrueMark.Library;
using TrueMarkApi.Client;
using TrueMarkCodesWorker;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Data.NHibernate.Mappings;
using Vodovoz.Infrastructure.Persistance;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;
using Vodovoz.Tools;
using VodovozBusiness.Models.TrueMark;
using AssemblyFinder = Vodovoz.Data.NHibernate.AssemblyFinder;
using DependencyInjection = Vodovoz.Data.NHibernate.DependencyInjection;

namespace TrueMarkCodePoolCheckWorker
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			await CreateHostBuilder(args).Build().RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
				{
					loggingBuilder.AddNLog();
					loggingBuilder.AddConfiguration(hostBuilderContext.Configuration.GetSection("NLog"));
				})
				.UseServiceProviderFactory(new AutofacServiceProviderFactory(ConfigureContainer))
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddMappingAssemblies(
							typeof(UserBaseMap).Assembly,
							typeof(AssemblyFinder).Assembly,
							typeof(Bank).Assembly,
							typeof(HistoryMain).Assembly,
							typeof(TypeOfEntity).Assembly,
							typeof(Attachment).Assembly,
							typeof(EmployeeWithLoginMap).Assembly
						)
						.AddDatabaseConnection()
						.AddNHibernateConventions()
						.AddCoreDataRepositories()
						.AddCore()
						.AddTrackedUoW()
						.AddInfrastructure()
						.AddTrackedUoW()

						.AddSingleton<ITrueMarkOrganizationClientSettingProvider>(sp =>
						{
							var configuration = sp.GetRequiredService<IConfiguration>();
							var trueMarkOrganizationClientSetting = new TrueMarkOrganizationClientSettingProvider(configuration.GetSection("TrueMarkOrganizationsClientSettings"));

							return trueMarkOrganizationClientSetting;
						})

						.AddCodesPool()

						.AddHostedService<CodePoolCheckWorker>()
						.AddTrueMarkApiClient()
						;

					DependencyInjection.AddStaticScopeForEntity(services);
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

			builder.RegisterType<TrueMarkWaterCodeParser>()
				.AsSelf()
				.InstancePerLifetimeScope();

			builder.RegisterInstance(ErrorReporter.Instance)
				.As<IErrorReporter>()
				.SingleInstance();

			builder.RegisterType<Tag1260Checker>()
				.AsSelf();
			
			builder.RegisterType<Tag1260Saver>()
				.AsSelf();
			
			builder.RegisterType<Tag1260Updater>()
				.AsSelf();
		}
	}
}
