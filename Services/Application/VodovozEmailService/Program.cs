using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using EmailService;
using Microsoft.Extensions.Configuration;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using NLog;
using QS.Project.DB;
using QSProjectsLib;
using Vodovoz.Core.DataService;
using Vodovoz.Parameters;

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
				var builder = new ConfigurationBuilder()
					.AddIniFile(configFile, optional: false);

				var configuration = builder.Build();

				var serviceSection = configuration.GetSection("Service");

				serviceHostName = serviceSection["service_host_name"];
				servicePort = serviceSection["service_port"];
				serviceWebPort = serviceSection["service_web_port"];

				var mysqlSection = configuration.GetSection("Mysql");
				mysqlServerHostName = mysqlSection["mysql_server_host_name"];
				mysqlServerPort = mysqlSection["mysql_server_port"];
				mysqlUser = mysqlSection["mysql_user"];
				mysqlPassword = mysqlSection["mysql_password"];
				mysqlDatabase = mysqlSection["mysql_database"];
			}
			catch (Exception ex) {
				logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}

			logger.Info("Запуск службы отправки электронной почты");
			try {
				var conStrBuilder = new MySqlConnectionStringBuilder();
				conStrBuilder.Server = mysqlServerHostName;
				conStrBuilder.Port = uint.Parse(mysqlServerPort);
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
					System.Reflection.Assembly.GetAssembly (typeof(EmailService.OrderEmail)),
					System.Reflection.Assembly.GetAssembly (typeof(QS.HistoryLog.HistoryMain)),
					System.Reflection.Assembly.GetAssembly (typeof(QS.Project.Domain.UserBase))
				});
				
				QS.HistoryLog.HistoryMain.Enable();

				EmailInstanceProvider emailInstanceProvider =
					new EmailInstanceProvider(new BaseParametersProvider(new ParametersProvider()));

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

				if (Environment.OSVersion.Platform == PlatformID.Unix)
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
