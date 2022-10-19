using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using InstantSmsService;
using Microsoft.Extensions.Configuration;
using Mono.Unix;
using Mono.Unix.Native;
using NLog;
using SmsRuSendService;

namespace VodovozInstantSmsService
{
    public class Service
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static readonly string configFile = "/etc/vodovoz-instant-sms-service.conf";

		//Service
		private static string serviceHostName;
		private static string servicePort;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			SmsRuConfiguration smsRuConfig;

			try {
				var builder = new ConfigurationBuilder()
					.AddIniFile(configFile, optional: false);

				var configuration = builder.Build();

				var serviceSection = configuration.GetSection("Service");
				serviceHostName = serviceSection["service_host_name"];
				servicePort = serviceSection["service_port"];

				var smsRuSection = configuration.GetSection("SmsRu");

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

			try {
				SmsRuSendController smsSender = new SmsRuSendController(smsRuConfig);

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
