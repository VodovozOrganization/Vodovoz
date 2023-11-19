using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Pacs.MangoCalls;
using Pacs.Server;
using QS.DomainModel.UoW;
using QS.Project.Core;
using QS.Project.DB;
using System.Reflection;
using Vodovoz.Core.Data.NHibernate.Mapping.Pacs;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.Settings.Pacs;

namespace Pacs.Calls.Service
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					var transportSettings = new TransportSettings();
					hostContext.Configuration.Bind("MessageTransport", transportSettings);

					services
						.AddSingleton<IMessageTransportSettings>(transportSettings)
						.AddCoreServerServices()
						.AddPacsOperatorServices()
						.AddSingleton<IUnitOfWorkFactory>(UnitOfWorkFactory.GetDefaultFactory)
						.AddPacsMangoCallsServices(transportSettings)
						.AddHostedService<CallsHostedService>();

					CreateBaseConfig();
				});
		}

		private static void CreateBaseConfig()
		{
			var conStrBuilder = new MySqlConnectionStringBuilder();


			conStrBuilder.Server = "dev.sql.vod.qsolution.ru";
			conStrBuilder.Port = 3307;
			conStrBuilder.Database = "Vodovoz_dev_test";
			conStrBuilder.UserID = "enzogord";
			conStrBuilder.Password = "dire5Dz8";
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
					Assembly.GetAssembly(typeof(OperatorMap))
				}
			);
		}
	}
}
