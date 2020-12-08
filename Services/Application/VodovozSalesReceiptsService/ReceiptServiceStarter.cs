using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Nini.Config;
using NLog;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Orders;

namespace VodovozSalesReceiptsService
{
	public static class ReceiptServiceStarter
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

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
				return;
			}

			logger.Info("Запуск службы фискализации и печати кассовых чеков...");

			var fiscalizationWorker = new FiscalizationWorker(
				OrderSingletonRepository.GetInstance(),
				new BaseParametersProvider(),
				new SalesReceiptSender()
			);
			fiscalizationWorker.Start();
			
			logger.Info("Служба фискализации запущена");

			var salesReceiptsInstanceProvider = new SalesReceiptsInstanceProvider(new BaseParametersProvider(), OrderSingletonRepository.GetInstance());
			WebServiceHost salesReceiptsHost = new SalesReceiptsServiceHost(salesReceiptsInstanceProvider);
			salesReceiptsHost.AddServiceEndpoint(
				typeof(ISalesReceiptsService),
				new WebHttpBinding(),
				$"http://{serviceHostName}:{servicePort}/SalesReceipts"
			);
			salesReceiptsHost.Open();
			logger.Info("Запущена служба мониторинга отправки чеков");

		}
	}
}