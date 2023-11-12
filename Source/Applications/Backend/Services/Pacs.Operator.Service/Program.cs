using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Pacs.Core;
using Pacs.Server;
using Pacs.Server.Consumers;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System.Reflection;
using Vodovoz.Core.Data.NHibernate.Mapping.Pacs;
using Vodovoz.Data.NHibernate.NhibernateExtensions;

namespace Pacs.Operator.Service
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
					services
						.AddSingleton<IUnitOfWorkFactory>(UnitOfWorkFactory.GetDefaultFactory)
						.AddPacsOperatorServices()
						.AddHostedService<OperatorHostedService>()

						.AddMassTransit(x =>
						{
							var entryAssembly = typeof(OperatorConnectConsumer).Assembly;
							x.AddConsumers(entryAssembly);

							x.UsingRabbitMq((context, cfg) =>
							{
								cfg.Host("localhost", 5672, "/", x =>
								{
								});

								cfg.ConfigurePublishTopology(context);

								cfg.ConfigureEndpoints(context);
							});
						});

					CreateBaseConfig();
				});

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
