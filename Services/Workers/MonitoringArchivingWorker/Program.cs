using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using NLog.Extensions.Logging;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Domain;
using System.Reflection;
using Vodovoz.EntityRepositories.HistoryChanges;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.NhibernateExtensions;
using Vodovoz.Parameters;
using QS.Attachments.Domain;
using Vodovoz.HibernateMapping;
using Microsoft.Extensions.Configuration;

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
				.ConfigureServices((hostContext, services) =>
				{
					services.AddLogging(loggingBuilder =>
					{
						loggingBuilder.ClearProviders();
						loggingBuilder.AddNLog("NLog.config");
					});

					services.AddSingleton<ISessionProvider, DefaultSessionProvider>();
					services.AddSingleton<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>();
					services.AddSingleton<IParametersProvider, ParametersProvider>();
					services.AddSingleton<IArchiveDataSettings, ArchiveDataSettings>();
					services.AddSingleton<ITrackRepository, TrackRepository>();
					services.AddSingleton<IOldTrackPointRepository, OldTrackPointRepository>();
					services.AddSingleton<IOldHistoryChangesRepository, OldHistoryChangesRepository>();
					services.AddSingleton<ICachedDistanceRepository, CachedDistanceRepository>();
					services.AddHostedService<MonitoringArchivingWorker>();

					CreateBaseConfig(hostContext.Configuration);
				});

		private static void CreateBaseConfig(IConfiguration configuration)
		{
			var conStrBuilder = new MySqlConnectionStringBuilder();

			var domainDBConfig = configuration.GetSection("DomainDB");

			conStrBuilder.Server = domainDBConfig.GetValue<string>("Server");
			conStrBuilder.Port = domainDBConfig.GetValue<uint>("Port");
			conStrBuilder.Database = domainDBConfig.GetValue<string>("Database");
			conStrBuilder.UserID = domainDBConfig.GetValue<string>("UserID");
			conStrBuilder.Password = domainDBConfig.GetValue<string>("Password");
			conStrBuilder.SslMode = MySqlSslMode.None;

			var connectionString = conStrBuilder.GetConnectionString(true);

			var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.ConnectionString(connectionString);

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				db_config,
				new Assembly[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(OrganizationMap)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment))
				}
			);
		}
	}
}
