using Microsoft.Extensions.Configuration;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using NLog;
using QS.Project.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading;
using Vodovoz.Settings.Database;

namespace VodovozSalesReceiptsService
{
	class Service
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private const string configFile = "/etc/vodovoz-sales-receipts-service.json";

		//Mysql
		private static string mysqlServerHostName;
		private static string mysqlServerPort;
		private static string mysqlUser;
		private static string mysqlPassword;
		private static string mysqlDatabase;

		//Service
		private static string serviceHostName;
		private static string servicePort;

		//ModulKassa
		private static string modulKassaBaseAddress;

		//Cashboxes
		private static IList<CashBox> cashboxes;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;

			logger.Info("Чтение конфигурационного файла...");

			const string configValueNotFoundString = "Не удалось прочитать значение параметра \"{0}\" из файла конфигурации";

			try
			{
				var builder = new ConfigurationBuilder()
					.AddJsonFile(configFile, false);
				var configuration = builder.Build();

				var serviceConfig = configuration.GetSection("Service");
				serviceHostName = serviceConfig["service_host_name"]
					?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "service_host_name"));
				servicePort = serviceConfig["service_port"]
					?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "service_port"));

				var modulKassaConfig = configuration.GetSection("ModulKassa");
				modulKassaBaseAddress = modulKassaConfig["base_address"]
					?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "test_base_address"));

				var mysqlConfig = configuration.GetSection("MySql");
				mysqlServerHostName = mysqlConfig["mysql_server_host_name"]
					?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "mysql_server_host_name"));
				mysqlServerPort = mysqlConfig["mysql_server_port"]
					?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "mysql_server_port"));
				mysqlUser = mysqlConfig["mysql_user"]
					?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "mysql_user"));
				mysqlPassword = mysqlConfig["mysql_password"]
					?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "mysql_password"));
				mysqlDatabase = mysqlConfig["mysql_database"]
					?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "mysql_database"));

				cashboxes = new List<CashBox>();
				var cashboxesConfig = configuration.GetSection("Cashboxes")?.GetChildren()
					?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "Cashboxes"));
				foreach(var cashboxConfig in cashboxesConfig)
				{
					string stringId = cashboxConfig["id"];
					if(string.IsNullOrWhiteSpace(stringId) || !int.TryParse(stringId, out int id))
					{
						throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "id"));
					}

					string stringUserName = cashboxConfig["user_name"];
					if(string.IsNullOrWhiteSpace(stringUserName) || !Guid.TryParse(stringUserName, out Guid userName))
					{
						throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "user_name"));
					}

					var cashBox = new CashBox
					{
						Id = id,
						UserName = userName,
						RetailPointName = cashboxConfig["retail_point_name"]
							?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "retail_point_name")),
						Password = cashboxConfig["password"]
							?? throw new ConfigurationErrorsException(string.Format(configValueNotFoundString, "password")),
					};
					cashboxes.Add(cashBox);
				}
				if(!cashboxes.Any())
				{
					throw new ConfigurationErrorsException(
						$"В конфигурационном файле не найдено данных ни для одной кассы ({nameof(CashBox)})");
				}
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}

			logger.Info("Настройка подключения к БД...");
			try {
				var conStrBuilder = new MySqlConnectionStringBuilder {
					Server = mysqlServerHostName,
					Port = UInt32.Parse(mysqlServerPort),
					Database = mysqlDatabase,
					UserID = mysqlUser,
					Password = mysqlPassword,
					SslMode = MySqlSslMode.None
				};
				
				var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
					.ConnectionString(conStrBuilder.GetConnectionString(true));

				OrmConfig.ConfigureOrm(
					dbConfig,
					new[]
					{
						Assembly.GetAssembly(typeof(QS.Banks.Domain.Bank)),
						Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.Organizations.OrganizationMap)),
						Assembly.GetAssembly(typeof(QS.HistoryLog.HistoryMain)),
						Assembly.GetAssembly(typeof(QS.Project.Domain.UserBase)),
						Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
						Assembly.GetAssembly(typeof(QS.Attachments.Domain.Attachment)),
						Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
					}
				);
				
				QS.HistoryLog.HistoryMain.Enable();
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка в настройке подключения к БД.");
				return;
			}

			try {
				ReceiptServiceStarter.StartService(serviceHostName, servicePort, modulKassaBaseAddress, cashboxes);
				
				if(Environment.OSVersion.Platform == PlatformID.Unix) {
					UnixSignal[] signals = {
						new UnixSignal (Signum.SIGINT),
						new UnixSignal (Signum.SIGHUP),
						new UnixSignal (Signum.SIGTERM)
					};
					UnixSignal.WaitAny(signals);
				}
				else {
					Console.ReadLine();
				}
			}
			catch(Exception e) {
				logger.Fatal(e);
			}
			finally {
				if(Environment.OSVersion.Platform == PlatformID.Unix)
				{
					Thread.CurrentThread.Abort();
				}
				Environment.Exit(0);
			}
		}

		static void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			logger.Fatal(e.ExceptionObject as Exception, "UnhandledException");
		}
	}
}
