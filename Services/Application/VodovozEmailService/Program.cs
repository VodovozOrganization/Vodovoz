using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Threading;
using EmailService;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using Nini.Config;
using NLog;
using QS.Project.DB;
using QSProjectsLib;
using QSSupportLib;
using Vodovoz.Core.DataService;

namespace VodovozEmailService
{
	class Service
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static readonly string configFile = "/etc/vodovoz-email-service.conf";

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

			logger.Info(String.Format("Запуск службы отправки электронной почты"));
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
					System.Reflection.Assembly.GetAssembly (typeof(EmailService.Email)),
					System.Reflection.Assembly.GetAssembly (typeof(QS.HistoryLog.HistoryMain)),
					System.Reflection.Assembly.GetAssembly (typeof(QS.Project.Domain.UserBase))
				});

				MainSupport.LoadBaseParameters();
				QS.HistoryLog.HistoryMain.Enable();

				EmailInstanceProvider emailInstanceProvider = new EmailInstanceProvider(new BaseParametersProvider());

				ServiceHost EmailSendingHost = new EmailServiceHost(emailInstanceProvider);
				ServiceHost MailjetEventsHost = new EmailServiceHost(emailInstanceProvider);

				ServiceEndpoint webEndPoint = EmailSendingHost.AddServiceEndpoint(
					typeof(IEmailServiceWeb),
					new WebHttpBinding(),
					String.Format("http://{0}:{1}/EmailServiceWeb", serviceHostName, serviceWebPort)
				);
				WebHttpBehavior httpBehavior = new WebHttpBehavior();
				webEndPoint.Behaviors.Add(httpBehavior);

				EmailSendingHost.AddServiceEndpoint(
					typeof(IEmailService),
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

				logger.Info("Server started.");

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

		static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			EmailManager.StopWorkers();
		}

		static void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
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
