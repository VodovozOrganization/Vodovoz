using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Mono.Unix;
using Mono.Unix.Native;
using MySqlConnector;
using NLog;
using QS.Project.DB;
using Sms.External.SmsRu;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.SmsNotifications;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;

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

		static NewClientSmsInformer newClientInformer;
		static UndeliveryNotApprovedSmsInformer undeliveryNotApprovedSmsInformer;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;;
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

			SmsRuConfiguration smsRuConfig;

			try {
				var builder = new ConfigurationBuilder()
					.AddIniFile(configFile, optional: false);

				var configuration = builder.Build();

				var serviceSection = configuration.GetSection("Service");
				serviceHostName = serviceSection["service_host_name"];
				servicePort = serviceSection["service_port"];

				var mysqlSection = configuration.GetSection("Mysql");
				mysqlServerHostName = mysqlSection["mysql_server_host_name"];
				mysqlServerPort = mysqlSection["mysql_server_port"];
				mysqlUser = mysqlSection["mysql_user"];
				mysqlPassword = mysqlSection["mysql_password"];
				mysqlDatabase = mysqlSection["mysql_database"];

				var smsRuSection = configuration.GetSection("SmsRu");

				smsRuConfig = 
					new SmsRuConfiguration(
						smsRuSection["login"],
						smsRuSection["password"],
						smsRuSection["appId"],
						smsRuSection["partnerId"],
						smsRuSection["email"],
						smsRuSection["smsNumberFrom"],
						smsRuSection["smtpLogin"],
						smsRuSection["smtpPassword"],
						smsRuSection["smtpServer"],
						int.Parse(smsRuSection["smtpPort"]),
						bool.Parse(smsRuSection["smtpUseSSL"]),
						bool.Parse(smsRuSection["translit"]),
						bool.Parse(smsRuSection["test"])
					);
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}

			try {
				var conStrBuilder = new MySqlConnectionStringBuilder();
				conStrBuilder.Server = mysqlServerHostName;
				conStrBuilder.Port = uint.Parse(mysqlServerPort);
				conStrBuilder.Database = mysqlDatabase;
				conStrBuilder.UserID = mysqlUser;
				conStrBuilder.Password = mysqlPassword;
				conStrBuilder.SslMode = MySqlSslMode.None;

				var db_config = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
										 .Dialect<NHibernate.Spatial.Dialect.MySQL57SpatialDialect>()
										 .ConnectionString(conStrBuilder.GetConnectionString(true));

				OrmConfig.ConfigureOrm(db_config,
					new Assembly[]
					{
						Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.Organizations.OrganizationMap)),
						Assembly.GetAssembly(typeof(QS.Banks.Domain.Bank)),
						Assembly.GetAssembly(typeof(QS.HistoryLog.HistoryMain)),
						Assembly.GetAssembly(typeof(QS.Project.Domain.UserBase)),
						Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
						Assembly.GetAssembly(typeof(QS.Attachments.Domain.Attachment)),
						Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
					});

				QS.HistoryLog.HistoryMain.Enable(conStrBuilder);

				ISmsNotificationRepository smsNotificationRepository = new SmsNotificationRepository();

				SmsRuSendController smsSender = new SmsRuSendController(smsRuConfig);
				
				newClientInformer = new NewClientSmsInformer(smsSender, smsNotificationRepository);
				newClientInformer.Start();

				BaseParametersProvider parametersProvider = new BaseParametersProvider(new ParametersProvider());
				LowBalanceNotifier lowBalanceNotifier = new LowBalanceNotifier(smsSender, smsSender, parametersProvider);
				lowBalanceNotifier.Start();
				
				undeliveryNotApprovedSmsInformer = new UndeliveryNotApprovedSmsInformer(smsSender, smsNotificationRepository);
				undeliveryNotApprovedSmsInformer.Start();

				SmsInformerInstanceProvider serviceStatusInstanceProvider = new SmsInformerInstanceProvider(
					smsNotificationRepository, 
					new BaseParametersProvider(new ParametersProvider())
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
			undeliveryNotApprovedSmsInformer.Stop();
		}
	}
}
