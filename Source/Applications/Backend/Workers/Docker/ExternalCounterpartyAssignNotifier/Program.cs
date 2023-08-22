using System;
using System.Reflection;
using System.Text.Json;
using ExternalCounterpartyAssignNotifier.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using NLog.Extensions.Logging;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Domain;
using Vodovoz.Data.NHibernate.NhibernateExtensions;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Settings.Database;

namespace ExternalCounterpartyAssignNotifier
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
					
					services.AddHttpClient<INotificationService, NotificationService>(client =>
					{
						client.Timeout = TimeSpan.FromSeconds(15);
					});

					services
						.AddSingleton<IExternalCounterpartyAssignNotificationRepository, ExternalCounterpartyAssignNotificationRepository>();
					services.AddSingleton(_ => new JsonSerializerOptions
					{
						PropertyNamingPolicy = JsonNamingPolicy.CamelCase
					});
					services.AddSingleton<ISessionProvider, DefaultSessionProvider>();
					services.AddSingleton<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>();
					services.AddHostedService<ExternalCounterpartyAssignNotifier>();
					
					CreateBaseConfig(hostContext.Configuration);
				});
		
		private static void CreateBaseConfig(IConfiguration configuration)
		{
			var domainDbConfig = configuration.GetSection("DomainDB");

			var conStrBuilder = new MySqlConnectionStringBuilder
			{
				Server = domainDbConfig["Server"],
				Port = domainDbConfig.GetValue<uint>("Port"),
				Database = domainDbConfig["Database"],
				UserID = domainDbConfig["UserID"],
				Password = domainDbConfig["Password"],
				SslMode = MySqlSslMode.None
			};

			var connectionString = conStrBuilder.GetConnectionString(true);

			var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
				.Dialect<MySQL57SpatialExtendedDialect>()
				.Driver<LoggedMySqlClientDriver>()
				.ConnectionString(connectionString);

			// Настройка ORM
			OrmConfig.ConfigureOrm(
				dbConfig,
				new[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(Vodovoz.Data.NHibernate.AssemblyFinder)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(HistoryMain)),
					Assembly.GetAssembly(typeof(TypeOfEntity)),
					Assembly.GetAssembly(typeof(Attachment)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				}
			);
		}
	}
}
