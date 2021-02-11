using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using System.Threading.Tasks;
using BitrixApi.DTO;
using BitrixApi.REST;
using BitrixIntegration;
using BitrixIntegration.DTO;
using BitrixIntegration.ServiceInterfaces;
using Mono.Unix;
using Mono.Unix.Native;
using MySql.Data.MySqlClient;
using Nini.Config;
using NLog;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QSProjectsLib;
using QSSupportLib;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBitrixIntegrationService
{
	class Service
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static readonly string configFile = "/home/gavr/vodovoz-bitrix-integration-service.conf"; //TODO gavr  вернуть на место

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
		
		//Bitrix
		private static string token;

		public static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += AppDomain_CurrentDomain_UnhandledException;
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
			logger.Info("Чтение конфигурационного файла...");
			// ReadConfig();

			#region ReadConfig

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
				
				IConfig bitrixConfig = confFile.Configs["Bitrix"];
				token = bitrixConfig.GetString("api_key");
			
				
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}

			#endregion ReadCOnfig
			logger.Info("Настройка подключения к БД...");
			ConfigureDBConnection();
			
			RunServiceLoop();
		}

		#region Configure

		//TODO gavr в отдельной функции не работает изза бага в Nini
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
				
				IConfig bitrixConfig = confFile.Configs["Bitrix"];
				token = bitrixConfig.GetString("api_key");
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
				
				QSMain.ConnectionString = conStrBuilder.GetConnectionString(true);
				var dbConfig = FluentNHibernate.Cfg.Db.MySQLConfiguration.Standard
					.ConnectionString(conStrBuilder.GetConnectionString(true)); 

				OrmConfig.ConfigureOrm(
					dbConfig,
					new[]
					{
						System.Reflection.Assembly.GetAssembly(typeof(Vodovoz.HibernateMapping.OrganizationMap)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Banks.Domain.Bank)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.HistoryLog.HistoryMain)),
						System.Reflection.Assembly.GetAssembly(typeof(QS.Project.Domain.UserBase))
					});

				MainSupport.LoadBaseParameters();
				QS.HistoryLog.HistoryMain.Enable();
			}
			catch (Exception ex)
			{
				logger.Fatal(ex, "Ошибка в настройке подключения к БД.");
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
		
		static async void StartService()
		{
			var bitrixInstanceProvider = new BitrixInstanceProvider(new BaseParametersProvider());

			var bitrixHost = new BitrixServiceHost(bitrixInstanceProvider);

			var webContract = typeof(IBitrixServiceWeb);
			var webBinding = new WebHttpBinding();
			var webAddress = $"http://{serviceHostName}:{serviceWebPort}/BitrixServiceWeb";
			var webEndPoint = bitrixHost.AddServiceEndpoint(webContract, webBinding, webAddress);
			
			WebHttpBehavior httpBehavior = new WebHttpBehavior();
			webEndPoint.Behaviors.Add(httpBehavior);

			var contract = typeof(IBitrixService);
			var binding = new BasicHttpBinding();
			var address = $"http://{serviceHostName}:{servicePort}/BitrixService";


			BitrixManager.SetToken(token);
//ТЕСТ текущих функций

			// BitrixManager.AddEvent(deal);
			var uow = UnitOfWorkFactory.CreateWithoutRoot();
				var cor = new CoR(token, BitrixRestApiFactory.CreateBitrixRestApi(token), uow, new Matcher());
				await cor.Process(138768);//138768 //150772
			
			// await tests();
			Console.ReadLine();
			
			bitrixHost.AddServiceEndpoint(contract, binding, address);
			
			bitrixHost.Open();
			logger.Log(LogLevel.Info, "Сервис запущен");

		

#if DEBUG
			// EmailSendingHost.Description.Behaviors.Add(new PreFilter());
			// MailjetEventsHost.Description.Behaviors.Add(new PreFilter());
#endif
			// EmailSendingHost.Open();
			// MailjetEventsHost.Open();
		}
		//
		// static async Task tests()
		// {
		// 	// Нужно ли умное сопоставление
		// 	bool needSearchOrder = false, 
		// 		needSearchPoint = false, 
		// 		needSearchNomenclature = false, 
		// 		needSearchCounterParty = false;
		// 	
		// 	//Нужно ли создавать если не сработало умное сопоставление
		// 	bool needCreateOrder = false,
		// 		needCreateDeliveryPoint = false,
		// 		needCreateNomenclature = false,
		// 		needCreateCounterParty = false;
		//
		// 	bool isCompany = false;
		//
		// 	#region Загрузка сущностей
		// 	
		// 	var bitrixApi = BitrixRestApiFactory.CreateBitrixRestApi(token);
		// 	
		// 	//Получаем сделку из битрикса
		// 	var deal = await bitrixApi.GetDealAsync(138768);
		// 	
		// 	//Определение это там контакт или компания
		// 	if (deal.CompanyId != 0)
		// 		isCompany = true;
		// 	else
		// 		isCompany = false;
		// 	
		// 	
		// 	//Получаем клиента из сделки
		// 	var contact = await bitrixApi.GetContact(deal.ContancId);
		//
		// 	//Получаем список товаров из сделки
		// 	var productList = await bitrixApi.GetProductsForDeal(deal.Id);
		// 	
		// 	#endregion Загрузка сущностей
		//
		// 	#region Проверка есть ли эти сущности у нас по BitrixId
		// 	
		// 	//ищем у нас сделку по битрикс Id
		// 	needCreateOrder = Matcher.MatchOrderByBitrixId(deal, out var ourOrder);
		// 	if (ourOrder != null)
		// 		return;
		// 	//ищем у нас контакт или компанию по битрикс Id
		// 	needCreateCounterParty = Matcher.MatchCounterpartyByBitrixId(contact.Id, out var ourCounterparty);
		// 	//точку доставки не ищем, её можно только начать сопоставлять тк кк она в сделке
		// 	//TODO удалить bitrixId у точек доставки из таблицы, сущности и маппинга 
		// 	
		// 	//ищем у нас товары по битрикс Id
		// 	IList<Nomenclature> matchedNomenclatures = new List<Nomenclature>();
		// 	IList<ProductFromDeal> unmatchedProducts = new List<ProductFromDeal>();
		// 	
		// 	foreach (var productBitrix in productList){
		// 		if (Matcher.MatchNomenclatureByBitrixId(productBitrix.Id, out var ourNomenclature)){
		// 			matchedNomenclatures.Add(ourNomenclature);
		// 		}
		// 		else{
		// 			unmatchedProducts.Add(productBitrix);
		// 		}
		// 	}
		//
		// 	if (unmatchedProducts.Count != 0)
		// 		needCreateNomenclature = true;
		//
		// 	#endregion BitrixId
		//
		// 	#region Для тех сущностей которых у нас еще нет, сопоставление
		// 	
		// 	
		// 	Counterparty counterparty = null;
		// 	//Сопоставление Counterparty by phone + secondName
		// 	Matcher.MatchCounterpartyByPhoneAndSecondName(contact, out counterparty);
  //           
		// 	//Сопоставление точки доставки для клиента
		// 	DeliveryPoint deliveryPoint = null;
		// 	Matcher.MatchDeliveryPoint(deal, counterparty, out deliveryPoint);
		//
		// 	
		// 	#endregion сопоставление
		// 	
		// 	#region Для тех сущностей который у нас нет, создание их с BitrixId 
		// 	
		// 	#endregion Создание
		// 	
		// 	#region Создание заказа
		// 	
		// 	#endregion Создание заказа
		// 	
		// 	
		// 	
		// 	
		// 	///////////////////
		// 	#region Сопоставление
		//
		// 	
		// 	
		//
		// 	
		//
		// 	#endregion Сопоставление
		// 	
		// 	#region Создаем сущности если нужны
		//
		// 	if (needCreateNomenclature){
		// 		
		// 	}
		// 	
		// 	if (needCreateDeliveryPoint){
		// 		
		// 	}
		// 	
		// 	if (needCreateCounterParty){
		// 		
		// 	}
		// 	
		// 	if (needCreateOrder){
		// 		
		// 	}
		// 	
		// 	
		// 	
		// 	#endregion
		//
		// }

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
