using Core.Infrastructure;
using MessageTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Pacs.MangoCalls;
using QS.DomainModel.UoW;
using QS.Project.Core;
using QS.Project.DB;
using System.Reflection;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Pacs;

namespace Pacs.Calls.Service
{
	public class Program
	{
		private const string _nLogSectionName = "NLog";
		private static DatabaseInfo _databaseInfo;

		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					/*var _loggerFactory = LoggerFactory.Create(logging =>
						logging.AddConfiguration(hostContext.Configuration.GetSection(_nLogSectionName)));

					var logger = _loggerFactory.CreateLogger<Program>();

					var sfsff = hostContext.Configuration.GetSection("MessageTransport")["Port"];
					logger.LogInformation($"Port: {sfsff}");*/


					var transportSettings = new ConfigTransportSettings();
					hostContext.Configuration.Bind("MessageTransport", transportSettings);

					services
						.AddCoreServerServices()

						//Настройки бд должны регистрироваться до настроек MassTransit
						.AddSettingsFromDatabase()

						.AddSingleton<IDataBaseInfo>(x => _databaseInfo)
						.AddSingleton<IUnitOfWorkFactory>(UnitOfWorkFactory.GetDefaultFactory)
						.AddSingleton<IMessageTransportSettings>(transportSettings)
						.AddPacsMangoCallsServices()
						;

					CreateBaseConfig(hostContext.Configuration);
				});
		}

		private static void CreateBaseConfig(IConfiguration configuration)
		{
			var dbSection = configuration.GetSection("DomainDB");
			var conStrBuilder = new MySqlConnectionStringBuilder();

			conStrBuilder.Server = dbSection.GetValue<string>("Server");
			conStrBuilder.Port = dbSection.GetValue<uint>("Port");
			conStrBuilder.Database = dbSection.GetValue<string>("Database");
			conStrBuilder.UserID = dbSection.GetValue<string>("UserID");
			conStrBuilder.Password = dbSection.GetValue<string>("Password");
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
					Assembly.GetAssembly(typeof(Vodovoz.Core.Data.NHibernate.AssemblyFinder)),
					Assembly.GetAssembly(typeof(SettingMap))
					
				}
			);

			_databaseInfo = new DatabaseInfo(conStrBuilder.Database);
		}
	}
}
