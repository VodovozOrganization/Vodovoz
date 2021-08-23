using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using Android;
using Microsoft.Extensions.Configuration;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using NLog;
using QS.Project.DB;
using QSProjectsLib;
using SmsPaymentService;
using Vodovoz.Models;
using Vodovoz.Parameters;

namespace VodovozSmsPaymentService
{
    internal static class Program
    { 
	    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
	    private static readonly string configFile = "/etc/vodovoz-sms-payment-service.conf";

		//Service
		private static string serviceHostName;
		private static string servicePort;
		private static string serviceWebPort;
		private static string driverServiceHostName;
		private static string driverServicePort;

		//Bitrix
		private static string baseAddress;

		//Mysql
		private static string mysqlServerHostName;
		private static string mysqlServerPort;
		private static string mysqlUser;
		private static string mysqlPassword;
		private static string mysqlDatabase;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;

			IConfiguration configuration;

			try
			{
				var builder = new ConfigurationBuilder()
					.AddIniFile(configFile, optional: false);

				configuration = builder.Build();

				var serviceSection = configuration.GetSection("Service");
				serviceHostName = serviceSection["service_host_name"];
				servicePort = serviceSection["service_port"];
				serviceWebPort = serviceSection["service_web_port"];
				driverServiceHostName = serviceSection["driver_service_host_name"];
				driverServicePort = serviceSection["driver_service_port"];

				var bitrixSection = configuration.GetSection("Bitrix");
				baseAddress = bitrixSection["base_address"];

				var mysqlSection = configuration.GetSection("Mysql");
				mysqlServerHostName = mysqlSection["mysql_server_host_name"];
				mysqlServerPort = mysqlSection["mysql_server_port"];
				mysqlUser = mysqlSection["mysql_user"];
				mysqlPassword = mysqlSection["mysql_password"];
				mysqlDatabase = mysqlSection["mysql_database"];
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}

			logger.Info("Запуск службы оплаты заказов по sms...");

			try {
				var conStrBuilder = new MySqlConnectionStringBuilder
				{
					Server = mysqlServerHostName,
					Port = UInt32.Parse(mysqlServerPort),
					Database = mysqlDatabase,
					UserID = mysqlUser,
					Password = mysqlPassword,
					SslMode = MySqlSslMode.None
				};

				QSMain.ConnectionString = conStrBuilder.GetConnectionString(true);
				var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
										 .Dialect<NHibernate.Spatial.Dialect.MySQL57SpatialDialect>()
										 .ConnectionString(QSMain.ConnectionString);
				
				OrmConfig.ConfigureOrm(dbConfig,
					new[] {
						System.Reflection.Assembly.GetAssembly (typeof(Vodovoz.HibernateMapping.OrganizationMap)),
						System.Reflection.Assembly.GetAssembly (typeof(QS.Banks.Domain.Bank)),
						System.Reflection.Assembly.GetAssembly (typeof(QS.HistoryLog.HistoryMain)),
						System.Reflection.Assembly.GetAssembly (typeof(QS.Project.Domain.UserBase))
					});

				QS.HistoryLog.HistoryMain.Enable();

				ChannelFactory<IAndroidDriverService> channelFactory = new ChannelFactory<IAndroidDriverService>(
					new BasicHttpBinding(),
					$"http://{driverServiceHostName}:{driverServicePort}/AndroidDriverService"
				);
				IDriverPaymentService driverPaymentService = new DriverPaymentService(channelFactory);
				ISmsPaymentStatusNotificationReciever smsPaymentStatusNotificationReciever = new DriverAPIHelper(configuration);
				var paymentSender = new BitrixPaymentController(baseAddress);
				
				var smsPaymentFileCache = new SmsPaymentFileCache("/tmp/VodovozSmsPaymentServiceTemp.txt");

				SmsPaymentServiceInstanceProvider smsPaymentServiceInstanceProvider = new SmsPaymentServiceInstanceProvider(
					paymentSender, 
					driverPaymentService,
					smsPaymentStatusNotificationReciever,
					new OrderParametersProvider(new ParametersProvider()),
					smsPaymentFileCache,
					new SmsPaymentDTOFactory(new OrderOrganizationProviderFactory().CreateOrderOrganizationProvider())
				);

				ServiceHost smsPaymentServiceHost = new SmsPaymentServiceHost(smsPaymentServiceInstanceProvider);
				
				ServiceEndpoint webEndPoint = smsPaymentServiceHost.AddServiceEndpoint(
					typeof(ISmsPaymentService),
					new WebHttpBinding(),
					$"http://{serviceHostName}:{serviceWebPort}/SmsPaymentWebService"
				);
				WebHttpBehavior httpBehavior = new WebHttpBehavior();
				webEndPoint.Behaviors.Add(httpBehavior);
				
				smsPaymentServiceHost.AddServiceEndpoint(
					typeof(ISmsPaymentService),
					new BasicHttpBinding(),
					$"http://{serviceHostName}:{servicePort}/SmsPaymentService"
				);
				smsPaymentServiceHost.Description.Behaviors.Add(new PreFilter());
				
				smsPaymentServiceHost.Open();
				logger.Info("Server started.");

				var serviceInstance = smsPaymentServiceInstanceProvider.GetInstance(null) as ISmsPaymentService;
				serviceInstance?.SynchronizePaymentStatuses();
				
				var unsavedPaymentsWorker = new CachePaymentsWorker(smsPaymentFileCache, serviceInstance);
				var overduePaymentsWorker = new OverduePaymentsWorker();
				unsavedPaymentsWorker.Start();
				overduePaymentsWorker.Start();

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

		static void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			logger.Fatal((Exception)e.ExceptionObject, "UnhandledException");
		}
    }

    #region Prefilter

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
		    foreach(var channelDispatcherBase in host.ChannelDispatchers)
		    {
			    var cDispatcher = (ChannelDispatcher) channelDispatcherBase;
			    foreach(EndpointDispatcher eDispatcher in cDispatcher.Endpoints)
				    eDispatcher.DispatchRuntime.MessageInspectors.Add(new ConsoleMessageTracer());
		    }
	    }
    }

    public class ConsoleMessageTracer : IDispatchMessageInspector
    {
	    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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

    #endregion
}
