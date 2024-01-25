using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.EntityRepositories.HistoryChanges;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Parameters;
using Vodovoz.Tools;

namespace MonitoringArchivingWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(new AutofacServiceProviderFactory())
				.ConfigureContainer<ContainerBuilder>(builder =>
				{
					builder.RegisterType<ParametersProvider>().AsImplementedInterfaces().SingleInstance();
					builder.RegisterType<ArchiveDataSettings>().AsImplementedInterfaces().SingleInstance();
					builder.RegisterType<TrackRepository>().AsImplementedInterfaces().SingleInstance();
					builder.RegisterType<ArchivedTrackPointRepository>().AsImplementedInterfaces().SingleInstance();
					builder.RegisterType<ArchivedHistoryChangesRepository>().AsImplementedInterfaces().SingleInstance();
					builder.RegisterType<CachedDistanceRepository>().AsImplementedInterfaces().SingleInstance();
					builder.RegisterType<DataArchiver>().AsImplementedInterfaces().SingleInstance();
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddLogging(logging =>
					{
						logging.ClearProviders();
						logging.AddNLog();
						logging.AddConfiguration(hostContext.Configuration.GetSection("NLog"));
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
						.AddHostedService<MonitoringArchivingWorker>()
						;
				});
	}
}
