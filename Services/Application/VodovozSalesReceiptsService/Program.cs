using System;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using Nini.Config;
using NLog;
using QS.Project.DB;
using QSProjectsLib;
using QSSupportLib;

namespace VodovozSalesReceiptsService
{
	class Service
	{
		static Logger logger = LogManager.GetCurrentClassLogger();
		static readonly string configFile = "/etc/vodovoz-sales-receipts-service.conf";

		//Service
		private static string serviceHostName;
		private static string servicePort;

		//Mysql
		static string mysqlServerHostName;
		static string mysqlServerPort;
		static string mysqlUser;
		static string mysqlPassword;
		static string mysqlDatabase;

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
				serviceHostName = serviceConfig.GetString("service_host_name");
				servicePort = serviceConfig.GetString("service_port");

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

				QSMain.ConnectionString = conStrBuilder.GetConnectionString(true);
				var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
												.ConnectionString(QSMain.ConnectionString)
												;

				OrmConfig.ConfigureOrm(
					db_config,
					new System.Reflection.Assembly[] {
						System.Reflection.Assembly.GetAssembly (typeof(QS.Banks.Domain.Bank)),
						System.Reflection.Assembly.GetAssembly (typeof(Vodovoz.HibernateMapping.OrganizationMap)),
						System.Reflection.Assembly.GetAssembly (typeof(QS.HistoryLog.HistoryMain)),
						System.Reflection.Assembly.GetAssembly (typeof(QS.Project.Domain.UserBase))
					}
				);

				MainSupport.LoadBaseParameters();
				QS.HistoryLog.HistoryMain.Enable();
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка в настройке подключения к БД.");
				return;
			}

			try {
				ReceiptServiceStarter.StartService(serviceConfig, kassaConfig);
				UnixSignal[] signals = {
					new UnixSignal (Signum.SIGINT),
					new UnixSignal (Signum.SIGHUP),
					new UnixSignal (Signum.SIGTERM)
				};
				UnixSignal.WaitAny(signals);
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
