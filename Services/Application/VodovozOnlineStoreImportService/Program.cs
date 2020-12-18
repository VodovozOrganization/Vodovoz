using System;
using System.Reflection;
using System.Threading;
using FluentNHibernate.Cfg.Db;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using NHibernate.Spatial.Dialect;
using Nini.Config;
using NLog;
using OnlineStoreImportService;
using QS.Project.DB;
using QSProjectsLib;
using QSSupportLib;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace VodovozOnlineStoreImportService
{
    internal class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static string driverConfigFile = "/etc/vodovoz-online-store-import-service.conf";
        
        private static string mysqlServerHostName;
        private static string mysqlServerPort;
        private static string mysqlUser;
        private static string mysqlPassword;
        private static string mysqlDatabase;
        
        public static void Main(string[] args)
        {
	        logger.Debug("Start");
            AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;

			try {
				IniConfigSource driverConfFile = new IniConfigSource(driverConfigFile);
				driverConfFile.Reload();
				
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
				var db_config = MySQLConfiguration.Standard
					.Dialect<MySQL57SpatialDialect>()
					.ConnectionString(QSMain.ConnectionString);

				OrmConfig.ConfigureOrm(db_config,
					new [] {
						Assembly.GetAssembly (typeof(QS.Banks.Domain.Bank)),
						Assembly.GetAssembly (typeof(Vodovoz.HibernateMapping.OrganizationMap)),
						Assembly.GetAssembly (typeof(QS.HistoryLog.HistoryMain)),
						Assembly.GetAssembly (typeof(QS.Project.Domain.UserBase))
				});

				MainSupport.LoadBaseParameters();
				QS.HistoryLog.HistoryMain.Enable();
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка в настройке подключения к БД.");
			}

			try {
				INomenclatureParametersProvider nomenclatureParametersProvider = new NomenclatureParametersProvider();
				INomenclatureRepository nomenclatureRepository = new NomenclatureRepository(nomenclatureParametersProvider);
				
				ImporterDataFromOnlineStore importer = new ImporterDataFromOnlineStore(
					nomenclatureParametersProvider,
					nomenclatureRepository
				);
				importer.Start();

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