using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using Nini.Config;
using NLog;
using QS.Project.DB;
using QSProjectsLib;
using QSSupportLib;
using SmsBlissSendService;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.SmsNotifications;

namespace VodovozSmsInformerService
{
	class Service
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static readonly string configFile = "/etc/vodovoz-smsinformer-service.conf";

		//Service
		private static string serviceHostName;
		private static string servicePort;

		//Mysql
		private static string mysqlServerHostName;
		private static string mysqlServerPort;
		private static string mysqlUser;
		private static string mysqlPassword;
		private static string mysqlDatabase;

		//SmsService
		private static string smsServiceLogin;
		private static string smsServicePassword;

		static NewClientSmsInformer newClientInformer;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;;
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;;

			try {
				IniConfigSource confFile = new IniConfigSource(configFile);
				confFile.Reload();

				IConfig serviceConfig = confFile.Configs["Service"];
				serviceHostName = serviceConfig.GetString("service_host_name");
				servicePort = serviceConfig.GetString("service_port");

				IConfig mysqlConfig = confFile.Configs["Mysql"];
				mysqlServerHostName = mysqlConfig.GetString("mysql_server_host_name");
				mysqlServerPort = mysqlConfig.GetString("mysql_server_port", "3306");
				mysqlUser = mysqlConfig.GetString("mysql_user");
				mysqlPassword = mysqlConfig.GetString("mysql_password");
				mysqlDatabase = mysqlConfig.GetString("mysql_database");

				IConfig smsConfig = confFile.Configs["SmsService"];
				smsServiceLogin = smsConfig.GetString("sms_service_login");
				smsServicePassword = smsConfig.GetString("sms_service_password");

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
					new System.Reflection.Assembly[] {
					System.Reflection.Assembly.GetAssembly (typeof(Vodovoz.HibernateMapping.OrganizationMap)),
					System.Reflection.Assembly.GetAssembly (typeof(QS.Banks.Domain.Bank)),
					System.Reflection.Assembly.GetAssembly (typeof(QS.HistoryLog.HistoryMain)),
					System.Reflection.Assembly.GetAssembly (typeof(QS.Project.Domain.UserBase))
				});

				MainSupport.LoadBaseParameters();
				QS.HistoryLog.HistoryMain.Enable();

				ISmsNotificationRepository smsNotificationRepository = new SmsNotificationRepository();

				SmsBlissSendController smsSender = new SmsBlissSendController(smsServiceLogin, smsServicePassword, SmsSendInterface.BalanceType.CurrencyBalance);
				newClientInformer = new NewClientSmsInformer(smsSender, smsNotificationRepository);
				newClientInformer.Start();

				BaseParametersProvider parametersProvider = new BaseParametersProvider();
				LowBalanceNotifier lowBalanceNotifier = new LowBalanceNotifier(smsSender, smsSender, parametersProvider);
				lowBalanceNotifier.Start();

				SmsInformerInstanceProvider serviceStatusInstanceProvider = new SmsInformerInstanceProvider(
					smsNotificationRepository, 
					new BaseParametersProvider()
				);
				WebServiceHost smsInformerStatus = new SmsInformerServiceHost(serviceStatusInstanceProvider);
				smsInformerStatus.AddServiceEndpoint(
					typeof(ISmsInformerService),
					new WebHttpBinding(),
					String.Format("http://{0}:{1}/SmsInformer", serviceHostName, servicePort)
				);
				smsInformerStatus.Open();
				logger.Info("Запущена служба мониторинга отправки смс уведомлений");

				UnixSignal[] signals = {
					new UnixSignal (Signum.SIGINT),
					new UnixSignal (Signum.SIGHUP),
					new UnixSignal (Signum.SIGTERM)};
				UnixSignal.WaitAny(signals);
			}
			catch(Exception ex) {
				logger.Fatal(ex);
			}
			finally {
				if(Environment.OSVersion.Platform == PlatformID.Unix)
					Thread.CurrentThread.Abort();
				Environment.Exit(0);
			}
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			logger.Fatal((Exception)e.ExceptionObject, "UnhandledException");
		}

		static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			newClientInformer?.Stop();
		}
	}
}
