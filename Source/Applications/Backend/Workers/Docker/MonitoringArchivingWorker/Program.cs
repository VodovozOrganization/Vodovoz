using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Extensions.Logging;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Domain;
using System.Reflection;
using Vodovoz.EntityRepositories.HistoryChanges;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Parameters;
using QS.Attachments.Domain;
using Microsoft.Extensions.Configuration;
using Vodovoz.Tools;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Vodovoz.Settings.Database;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using QS.Project.Core;

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
					services.AddLogging(loggingBuilder =>
					{
						loggingBuilder.ClearProviders();
						loggingBuilder.AddNLog("NLog.config");
					});

					services
						.AddCore()
						.AddTrackedUoW()
						.AddHostedService<MonitoringArchivingWorker>()
						;

					CreateBaseConfig(services);
				});

		private static void CreateBaseConfig(IServiceCollection services)
		{
			var serviceProvider = services.BuildServiceProvider();
			var configuration = serviceProvider.GetRequiredService<IConfiguration>();


			var domainDBConfig = configuration.GetSection("DomainDB");

			var conStrBuilder = new MySqlConnectionStringBuilder();
			conStrBuilder.Server = domainDBConfig.GetValue<string>("Server");
			conStrBuilder.Port = domainDBConfig.GetValue<uint>("Port");
			conStrBuilder.Database = domainDBConfig.GetValue<string>("Database");
			conStrBuilder.UserID = domainDBConfig.GetValue<string>("UserID");
			conStrBuilder.Password = domainDBConfig.GetValue<string>("Password");
			conStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = conStrBuilder.GetConnectionString(true);

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.Driver<LoggedMySqlClientDriver>()
				.ConnectionString(connectionString);

			var ormConfig = serviceProvider.GetRequiredService<IOrmConfig>();
			ormConfig.ConfigureOrm(
				db_config,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(AssemblyFinder))
				}
			);
		}
	}
}
