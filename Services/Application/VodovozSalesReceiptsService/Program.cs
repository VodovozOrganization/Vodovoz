using System;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using Nini.Config;
using NLog;
using QS.Project.DB;

namespace VodovozSalesReceiptsService
{
	class Service
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		private const string configFile = "/etc/vodovoz-sales-receipts-service.conf";

		//Mysql
		private static string mysqlServerHostName;
		private static string mysqlServerPort;
		private static string mysqlUser;
		private static string mysqlPassword;
		private static string mysqlDatabase;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;

			logger.Info("Чтение конфигурационного файла...");
			IConfig serviceConfig;
			IConfig kassaConfig;
			
			try {
				IniConfigSource confFile = new IniConfigSource(configFile);
				confFile.Reload();
				serviceConfig = confFile.Configs["Service"];
				kassaConfig = confFile.Configs["ModulKassa"];

				IConfig mysqlConfig = confFile.Configs["Mysql"];
				mysqlServerHostName = mysqlConfig.GetString("mysql_server_host_name");
				mysqlServerPort = mysqlConfig.GetString("mysql_server_port", "3306");
				mysqlUser = mysqlConfig.GetString("mysql_user");
				mysqlPassword = mysqlConfig.GetString("mysql_password");
				mysqlDatabase = mysqlConfig.GetString("mysql_database");

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
					new[] {
						System.Reflection.Assembly.GetAssembly (typeof(QS.Banks.Domain.Bank)),
						System.Reflection.Assembly.GetAssembly (typeof(Vodovoz.HibernateMapping.OrganizationMap)),
						System.Reflection.Assembly.GetAssembly (typeof(QS.HistoryLog.HistoryMain)),
						System.Reflection.Assembly.GetAssembly (typeof(QS.Project.Domain.UserBase))
					}
				);
				
				QS.HistoryLog.HistoryMain.Enable();
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка в настройке подключения к БД.");
				return;
			}

			try {
				ReceiptServiceStarter.StartService(serviceConfig, kassaConfig);
				
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
					Thread.CurrentThread.Abort();
				Environment.Exit(0);
			}
		}

		static void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			logger.Fatal(e.ExceptionObject as Exception, "UnhandledException");
		}
	}
}
