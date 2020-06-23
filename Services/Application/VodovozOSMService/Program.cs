using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;
using Nini.Config;
using NLog;
using QS.Osm;

namespace VodovozOSMService
{
	class Service
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private static string configFile = "/etc/vodovoz-osm-service.conf";

		//Service
		private static string serviceHostName;
		private static string servicePort;

		private static System.Timers.Timer orderRoutineTimer;
		private static System.Timers.Timer trackRoutineTimer;
		private static System.Timers.Timer onlineStoreCatalogSyncTimer;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;
			try {
				IniConfigSource confFile = new IniConfigSource(configFile);
				confFile.Reload();
				IConfig serviceConfig = confFile.Configs["Service"];
				serviceHostName = serviceConfig.GetString("service_host_name");
				servicePort = serviceConfig.GetString("service_port");

				OsmService.ConfigureService(confFile);
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}

			logger.Info(String.Format("Запуск службы OSM"));

			WebServiceHost OsmHost = new WebServiceHost(typeof(OsmService));

			try {
				OsmWorker.ServiceHost = serviceHostName;
				OsmWorker.ServicePort = Int32.Parse(servicePort);
				OsmHost.AddServiceEndpoint(
					typeof(IOsmService),
					new WebHttpBinding(),
					OsmWorker.ServiceAddress
				);

#if DEBUG
				OsmHost.Description.Behaviors.Add(new PreFilter());
#endif

				OsmHost.Open();

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
				if(OsmHost.State == CommunicationState.Opened)
					OsmHost.Close();

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
