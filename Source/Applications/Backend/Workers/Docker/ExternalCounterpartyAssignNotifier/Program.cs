using System.Reflection;
using ExternalCounterpartyAssignNotifier.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using NLog.Extensions.Logging;
using QS.Attachments.Domain;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.DB;
using QS.Project.Domain;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.HibernateMapping.Organizations;
using Vodovoz.NhibernateExtensions;
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
					
					services.AddHttpClient<INotificationService, NotificationService>(c =>
					{
						//c.BaseAddress = new Uri(hostContext.Configuration.GetSection("VodovozSiteNotificationService").GetValue<string>("BaseUrl"));
						c.DefaultRequestHeaders.Add("Accept", "application/json");
					});

					services
						.AddSingleton<IExternalCounterpartyAssignNotificationRepository, ExternalCounterpartyAssignNotificationRepository>();
					services.AddSingleton<ISessionProvider, DefaultSessionProvider>();
					services.AddSingleton<IUnitOfWorkFactory, DefaultUnitOfWorkFactory>();
					services.AddHostedService<ExternalCounterpartyAssignNotifier>();
					
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
				new[]
				{
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.UserBaseMap)),
					Assembly.GetAssembly(typeof(OrganizationMap)),
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
