using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using NLog;
using Nini.Config;

namespace VodovozMobileService
{
	public static class MobileServiceStarter
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private static System.Timers.Timer onlineStoreCatalogSyncTimer;

		public static void StartService(IConfig serviceConfig)
		{
			string serviceHostName;
			string servicePort;

			try {
				serviceHostName = serviceConfig.GetString("service_host_name");
				servicePort = serviceConfig.GetString("service_port");
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				throw;
			}

			logger.Info(String.Format("Запуск службы мобильного приложения"));

			WebServiceHost mobileHost = new WebServiceHost(typeof(MobileService));

			MobileService.BaseUrl = String.Format("http://{0}:{1}/Mobile", serviceHostName, servicePort);
			mobileHost.AddServiceEndpoint(
				typeof(IMobileService),
				new WebHttpBinding(),
				MobileService.BaseUrl
			);

			//FIXME Тут добавлен без дебага, потому что без него не работает отдача изображений в потоке. Метод Stream GetImage(string filename)
			// Просто не смог быстро разобраться. А конкретнее нужна строка reply = TraceMessage (reply.CreateBufferedCopy (int.MaxValue), Action.Send);
			// видимо она как то обрабатывает сообщение.
			mobileHost.Description.Behaviors.Add(new PreFilter());

			mobileHost.Open();

			//Запускаем таймеры рутины
			onlineStoreCatalogSyncTimer = new System.Timers.Timer(3600000); //1 час
			onlineStoreCatalogSyncTimer.Elapsed += OnlineStoreCatalogSyncTimer_Elapsed;
			onlineStoreCatalogSyncTimer.Start();

			logger.Info("Server started.");
		}

		private static bool onlineStoreSyncRunning = false;

		static void OnlineStoreCatalogSyncTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if(onlineStoreSyncRunning)
				return;

			//Выполняем сихнронизацию только с 8 до 23.
			if(DateTime.Now.Hour < 7 || DateTime.Now.Hour > 23)
				return;

			try {
				onlineStoreSyncRunning = true;
				BackgroundTask.OnlineStoreCatalogSync();
			}
			catch(Exception ex) {
				logger.Error(ex, "Исключение при выполение фоновой задачи.");
			}
			finally {
				onlineStoreSyncRunning = false;
			}
		}

		static void AppDomain_CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			logger.Fatal((Exception)e.ExceptionObject, "UnhandledException");
		}
	}
}
