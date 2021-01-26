using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using BitrixIntegration;
using BitrixIntegration.DTO;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using Nini.Config;
using NLog;
using QS.Project.DB;
using Vodovoz.Core.DataService;

namespace VodovozBitrixIntegrationService
{
	class Service
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static readonly string configFile = "/home/gavr/vodovoz-bitrix-integration-service.conf"; //TODO gavr 

		//Service
		private static string serviceHostName;
		private static string servicePort;
		private static string serviceWebPort;

		//Mysql
		private static string mysqlServerHostName;
		private static string mysqlServerPort;
		private static string mysqlUser;
		private static string mysqlPassword;
		private static string mysqlDatabase;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
			logger.Info("Чтение конфигурационного файла...");
			ReadConfig();
			logger.Info("Настройка подключения к БД...");
			ConfigureDBConnection();
			RunServiceLoop();
		}

		#region Configure

		static void ReadConfig()
		{
			try {
				IniConfigSource confFile = new IniConfigSource(configFile);
				confFile.Reload();
				IConfig serviceConfig = confFile.Configs["Service"];
				serviceHostName = serviceConfig.GetString("service_host_name");
				servicePort = serviceConfig.GetString("service_port");
				serviceWebPort = serviceConfig.GetString("service_web_port");

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
		}

		static void ConfigureDBConnection()
		{
			try
			{
				var conStrBuilder = new MySqlConnectionStringBuilder
				{
					Server = mysqlServerHostName,
					Port = UInt32.Parse(mysqlServerPort),
					Database = mysqlDatabase,
					UserID = mysqlUser,
					Password = mysqlPassword,
					SslMode = MySqlSslMode.None
				};
				
				var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
					.ConnectionString(conStrBuilder.GetConnectionString(true));

				OrmConfig.ConfigureOrm(dbConfig,
					new[]
					{
						System.Reflection.Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.OrganizationMap)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Banks.Domain.Bank)),
						System.Reflection.Assembly.GetAssembly(typeof(Email)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.HistoryLog.HistoryMain)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Project.Domain.UserBase))
					});

				// MainSupport.LoadBaseParameters();
				QS.HistoryLog.HistoryMain.Enable();
			}
			catch (Exception ex)
			{
				logger.Fatal(ex, "Ошибка в настройке подключения к БД.");
				return;
			}
		}


		#endregion

		#region StartService
		static void RunServiceLoop()
		{
			try
			{
				StartService();
				logger.Info("Server started.");

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
		
		static void StartService()
		{
			BitrixInstanceProvider bitrixInstanceProvider = new BitrixInstanceProvider(new BaseParametersProvider());

			ServiceHost EmailSendingHost = new BitrixServiceHost(bitrixInstanceProvider);
			ServiceHost MailjetEventsHost = new BitrixServiceHost(bitrixInstanceProvider);

			ServiceEndpoint webEndPoint = EmailSendingHost.AddServiceEndpoint(
				typeof(IBitrixServiceWeb),
				new WebHttpBinding(),
				String.Format("http://{0}:{1}/EmailServiceWeb", serviceHostName, serviceWebPort)
			);
			WebHttpBehavior httpBehavior = new WebHttpBehavior();
			webEndPoint.Behaviors.Add(httpBehavior);

			EmailSendingHost.AddServiceEndpoint(
				typeof(IBitrixService),
				new BasicHttpBinding(),
				String.Format("http://{0}:{1}/EmailService", serviceHostName, servicePort)
			);

			var mailjetEndPoint = MailjetEventsHost.AddServiceEndpoint(
				typeof(IMailjetEventService),
				new WebHttpBinding(),
				String.Format("http://{0}:{1}/Mailjet", serviceHostName, servicePort)
			);
			WebHttpBehavior mailjetHttpBehavior = new WebHttpBehavior();
			mailjetEndPoint.Behaviors.Add(httpBehavior);

#if DEBUG
			EmailSendingHost.Description.Behaviors.Add(new PreFilter());
			MailjetEventsHost.Description.Behaviors.Add(new PreFilter());
#endif
			EmailSendingHost.Open();
			MailjetEventsHost.Open();
		}
		
		#endregion StartService

		#region Signals

		static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			BitrixManager.StopWorkers();
		}

		static void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			logger.Fatal((Exception)e.ExceptionObject, "UnhandledException");
		}

		#endregion
		
	}
}
