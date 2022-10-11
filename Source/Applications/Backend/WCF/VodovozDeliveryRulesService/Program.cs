using Fias.Service;
using Fias.Service.Cache;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using Nini.Config;
using NLog;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Settings.Database;

namespace VodovozDeliveryRulesService
{
	class Service
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private const string _configFile = "/etc/vodovoz-delivery-rules-service.conf";

		//Service
		private static string _serviceHostName;
		private static string _servicePort;

		//Mysql
		private static string _mysqlServerHostName;
		private static string _mysqlServerPort;
		private static string _mysqlUser;
		private static string _mysqlPassword;
		private static string _mysqlDatabase;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;

			try {
				IniConfigSource confFile = new IniConfigSource(_configFile);
				confFile.Reload();
				IConfig serviceConfig = confFile.Configs["Service"];
				_serviceHostName = serviceConfig.GetString("service_host_name");
				_servicePort = serviceConfig.GetString("service_port");

				IConfig mysqlConfig = confFile.Configs["Mysql"];
				_mysqlServerHostName = mysqlConfig.GetString("mysql_server_host_name");
				_mysqlServerPort = mysqlConfig.GetString("mysql_server_port", "3306");
				_mysqlUser = mysqlConfig.GetString("mysql_user");
				_mysqlPassword = mysqlConfig.GetString("mysql_password");
				_mysqlDatabase = mysqlConfig.GetString("mysql_database");
			}
			catch(Exception ex) {
				_logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}

			_logger.Info("Запуск службы правил доставки...");
			try {
				var conStrBuilder = new MySqlConnectionStringBuilder();
				conStrBuilder.Server = _mysqlServerHostName;
				conStrBuilder.Port = UInt32.Parse(_mysqlServerPort);
				conStrBuilder.Database = _mysqlDatabase;
				conStrBuilder.UserID = _mysqlUser;
				conStrBuilder.Password = _mysqlPassword;
				conStrBuilder.SslMode = MySqlSslMode.None;

				var connectionString = conStrBuilder.GetConnectionString(true);
				var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
										 .Dialect<NHibernate.Spatial.Dialect.MySQL57SpatialDialect>()
										 .ConnectionString(connectionString);
				var assemblies = new[]
				{
					Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.Organizations.OrganizationMap)),
					Assembly.GetAssembly(typeof(Bank)),
					Assembly.GetAssembly(typeof(QS.Project.Domain.UserBase)),
					Assembly.GetAssembly(typeof(QS.Project.HibernateMapping.TypeOfEntityMap)),
					Assembly.GetAssembly(typeof(QS.Attachments.Domain.Attachment)),
					Assembly.GetAssembly(typeof(VodovozSettingsDatabaseAssemblyFinder))
				};

				OrmConfig.ConfigureOrm(dbConfig, assemblies);

				IDeliveryRepository deliveryRepository = new DeliveryRepository();
				var backupDistrictService = new BackupDistrictService();
				IDeliveryRulesParametersProvider deliveryRulesParametersProvider
					= new DeliveryRulesParametersProvider(new ParametersProvider());
				CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
				IFiasApiParametersProvider fiasApiParametersProvider = new FiasApiParametersProvider(new ParametersProvider());
				var geoCoderCache = new GeocoderCache(UnitOfWorkFactory.GetDefaultFactory);
				IFiasApiClient fiasApiClient = new FiasApiClient(fiasApiParametersProvider.FiasApiBaseUrl, fiasApiParametersProvider.FiasApiToken, geoCoderCache);
				DeliveryRulesInstanceProvider deliveryRulesInstanceProvider = 
					new DeliveryRulesInstanceProvider(deliveryRepository, backupDistrictService, deliveryRulesParametersProvider, fiasApiClient, cancellationTokenSource);
				ServiceHost deliveryRulesHost = new DeliveryRulesServiceHost(deliveryRulesInstanceProvider);

				ServiceEndpoint webEndPoint = deliveryRulesHost.AddServiceEndpoint(
					typeof(IDeliveryRulesService),
					new WebHttpBinding(),
					$"http://{_serviceHostName}:{_servicePort}/DeliveryRules"
				);
				WebHttpBehavior httpBehavior = new WebHttpBehavior();
				webEndPoint.Behaviors.Add(httpBehavior);

#if DEBUG
				deliveryRulesHost.Description.Behaviors.Add(new PreFilter());
#endif
				backupDistrictService.StartAutoUpdateTask();
				
				deliveryRulesHost.Open();

				_logger.Info("Server started.");

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
			catch(Exception e) {
				_logger.Fatal(e);
			}
			finally {
				if(Environment.OSVersion.Platform == PlatformID.Unix)
				{
					Thread.CurrentThread.Abort();
				}
				Environment.Exit(0);
			}
		}

		static void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			_logger.Fatal((Exception)e.ExceptionObject, "UnhandledException");
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
			foreach(var channelDispatcherBase in host.ChannelDispatchers)
			{
				var cDispatcher = (ChannelDispatcher)channelDispatcherBase;
				foreach(EndpointDispatcher eDispatcher in cDispatcher.Endpoints)
				{
					eDispatcher.DispatchRuntime.MessageInspectors.Add(new ConsoleMessageTracer());
				}
			}
		}
	}

	public class ConsoleMessageTracer : IDispatchMessageInspector
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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
					_logger.Info("Received: {0}", msg.Headers.To.AbsoluteUri);
					if(!msg.IsEmpty)
					{
						_logger.Debug("Received Body: {0}", msg);
					}
				} else
				{
					_logger.Debug("Sended: {0}", msg);
				}
			}
			catch(Exception ex) {
				_logger.Error(ex, "Ошибка логгирования сообщения.");
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
