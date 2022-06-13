using InstantSmsService;
using MegafonSmsSendService;
using Microsoft.Extensions.Configuration;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using NLog;
using QS.Project.DB;
using SmsRuSendService;
using SmsSendInterface;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace VodovozInstantSmsService
{
	public class Service
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static readonly string configFile = "/etc/vodovoz-instant-sms-service.conf";

		//Service
		private static string serviceHostName;
		private static string servicePort;

		//Mysql
		private static string mysqlServerHostName;
		private static string mysqlServerPort;
		private static string mysqlUser;
		private static string mysqlPassword;
		private static string mysqlDatabase;

		private static IConfigurationSection megafonSmsSection;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			SmsRuConfiguration smsRuConfig;

			const string configValueNotFoundString = "Не удалось прочитать значение параметра \"{0}\" из файла конфигурации";

			try
			{
				var builder = new ConfigurationBuilder()
					.AddIniFile(configFile, optional: false);

				var configuration = builder.Build();

				var serviceSection = configuration.GetSection("Service");
				serviceHostName = serviceSection["service_host_name"];
				servicePort = serviceSection["service_port"];

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

				var smsRuSection = configuration.GetSection("SmsRu");
				megafonSmsSection = configuration.GetSection("MegafonSms");

				smsRuConfig = new SmsRuConfiguration(
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

			logger.Info("Настройка подключения к БД...");
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

				OrmConfig.ConfigureOrm(
					dbConfig,
					new[]
					{
						System.Reflection.Assembly.GetAssembly(typeof(QS.Banks.Domain.Bank)),
						System.Reflection.Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.OrganizationMap)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.HistoryLog.HistoryMain)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Project.Domain.UserBase)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Attachments.Domain.Attachment))
					}
				);

				QS.HistoryLog.HistoryMain.Enable();
			}
			catch(Exception ex)
			{
				logger.Fatal(ex, "Ошибка в настройке подключения к БД.");
				return;
			}

			try {
				ISmsSender smsSender;
				ISmsBalanceNotifier smsBalanceNotifier;

				var smsSettings = new SmsSettings(new ParametersProvider());
				switch(smsSettings.SmsProvider)
				{
					case SmsProvider.SmsRu:
						var smsRuSender = new SmsRuSendController(smsRuConfig);
						smsSender = smsRuSender;
						smsBalanceNotifier = smsRuSender;
						break;
					case SmsProvider.Megafon:
					default:
						var login = megafonSmsSection["login"];
						var password = megafonSmsSection["password"];
						var megafonSender = new MegafonSmsSender(login, password, smsSettings);
						smsSender = megafonSender;
						smsBalanceNotifier = megafonSender;
						break;
				}

				InstantSmsServiceInstanceProvider instantSmsInstanceProvider = new InstantSmsServiceInstanceProvider(smsSender);
				ServiceHost InstantSmsServiceHost = new InstantSmsServiceHost(instantSmsInstanceProvider);

				var webEndPoint = InstantSmsServiceHost.AddServiceEndpoint(
					typeof(IInstantSmsService),
					new WebHttpBinding(),
					String.Format("http://{0}:{1}/InstantSmsInformer", serviceHostName, servicePort)
				);
				WebHttpBehavior httpBehavior = new WebHttpBehavior();
				webEndPoint.Behaviors.Add(httpBehavior);

				InstantSmsServiceHost.AddServiceEndpoint(
				typeof(IInstantSmsService),
				new BasicHttpBinding(),
				String.Format("http://{0}:{1}/InstantSmsService", serviceHostName, servicePort)
				);
#if DEBUG
				InstantSmsServiceHost.Description.Behaviors.Add(new PreFilter());
#endif
				InstantSmsServiceHost.Open();

				logger.Info("Запущена служба отправки моментальных sms сообщений");

				if(Environment.OSVersion.Platform == PlatformID.Unix)
				{
					UnixSignal[] signals = {
						new UnixSignal (Signum.SIGINT),
						new UnixSignal (Signum.SIGHUP),
						new UnixSignal (Signum.SIGTERM)
					};
					UnixSignal.WaitAny(signals);
				}
				else
				{
					Console.ReadLine();
				}
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
	}

	public class PreFilter : IServiceBehavior
	{
		public void AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
		{
		}

		public void Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription desc, ServiceHostBase host)
		{
			foreach(ChannelDispatcher cDispatcher in host.ChannelDispatchers)
				foreach(EndpointDispatcher eDispatcher in cDispatcher.Endpoints)
					eDispatcher.DispatchRuntime.MessageInspectors.Add(new ConsoleMessageTracer());
		}
	}

	public class ConsoleMessageTracer : IDispatchMessageInspector
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		enum Action
		{
			Send,
			Receive
		};

		private Message TraceMessage(MessageBuffer buffer, Action action)
		{
			Message msg = buffer.CreateMessage();
			try {
				if(action == Action.Receive) {
					logger.Info("Received: {0}", msg.Headers.To.AbsoluteUri);
					if(!msg.IsEmpty)
						logger.Debug("Received Body: {0}", msg);
				} else
					logger.Debug("Sended: {0}", msg);
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка логгирования сообщения.");
			}
			return buffer.CreateMessage();
		}

		public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
		{
			request = TraceMessage(request.CreateBufferedCopy(int.MaxValue), Action.Receive);
			return null;
		}

		public void BeforeSendReply(ref Message reply, object correlationState)
		{
			reply = TraceMessage(reply.CreateBufferedCopy(int.MaxValue), Action.Send);
		}
	}
}
