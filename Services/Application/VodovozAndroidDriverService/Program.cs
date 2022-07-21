using System;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using Nini.Config;
using NLog;
using QS.Project.DB;
using QSProjectsLib;
using Vodovoz.Core.DataService;
using Vodovoz.Parameters;

namespace VodovozAndroidDriverService
{
	class Service
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static string driverConfigFile = "/etc/vodovoz-driver-service.conf";

		//Mysql
		private static string mysqlServerHostName;
		private static string mysqlServerPort;
		private static string mysqlUser;
		private static string mysqlPassword;
		private static string mysqlDatabase;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;

			IConfig driverServiceConfig;
			IConfig firebaseConfig;

			try {
				IniConfigSource driverConfFile = new IniConfigSource(driverConfigFile);
				driverConfFile.Reload();
				driverServiceConfig = driverConfFile.Configs["Service"];
				firebaseConfig = driverConfFile.Configs["Firebase"];

				IConfig mysqlConfig = driverConfFile.Configs["Mysql"];
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

			try {
				var conStrBuilder = new MySqlConnectionStringBuilder();
				conStrBuilder.Server = mysqlServerHostName;
				conStrBuilder.Port = UInt32.Parse(mysqlServerPort);
				conStrBuilder.Database = mysqlDatabase;
				conStrBuilder.UserID = mysqlUser;
				conStrBuilder.Password = mysqlPassword;
				conStrBuilder.SslMode = MySqlSslMode.None;

				QSMain.ConnectionString = conStrBuilder.GetConnectionString(true);
				var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
										 .Dialect<NHibernate.Spatial.Dialect.MySQL57SpatialDialect>()
										 .ConnectionString(QSMain.ConnectionString);

				OrmConfig.ConfigureOrm(db_config,
					new System.Reflection.Assembly[]
					{
						System.Reflection.Assembly.GetAssembly(typeof(QS.Banks.Domain.Bank)),
						System.Reflection.Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.Organizations.OrganizationMap)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.HistoryLog.HistoryMain)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Project.Domain.UserBase)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Attachments.Domain.Attachment))
					});

				QS.HistoryLog.HistoryMain.Enable();
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка в настройке подключения к БД.");
			}


			try {
				DriverServiceStarter.StartService(driverServiceConfig, firebaseConfig, new BaseParametersProvider(new ParametersProvider()));

				UnixSignal[] signals = {
					new UnixSignal (Signum.SIGINT),
					new UnixSignal (Signum.SIGHUP),
					new UnixSignal (Signum.SIGTERM)};
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
			logger.Fatal((Exception)e.ExceptionObject, "UnhandledException");
		}
	}
}
