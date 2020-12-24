using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Nini.Config;
using NLog;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Parameters;

namespace VodovozSalesReceiptsService
{
	public static class ReceiptServiceStarter
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static void StartService(IConfig serviceConfig, IConfig kassaConfig)
		{
			string serviceHostName;
			string servicePort;
			string baseAddress;

			try {
				serviceHostName = serviceConfig.GetString("service_host_name");
				servicePort = serviceConfig.GetString("service_port");
				
				baseAddress = kassaConfig.GetString("base_address");
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}

			logger.Info("Запуск службы фискализации и печати кассовых чеков...");

			var fiscalizationWorker = new FiscalizationWorker(
				OrderSingletonRepository.GetInstance(),
				new SalesReceiptSender(baseAddress),
				new OrderPrametersProvider(ParametersProvider.Instance),
				new OrganizationParametersProvider(ParametersProvider.Instance)
			);
			fiscalizationWorker.Start();
			
			logger.Info("Служба фискализации запущена");

			var salesReceiptsInstanceProvider = new SalesReceiptsInstanceProvider(
				new BaseParametersProvider(),
				OrderSingletonRepository.GetInstance(),
				new OrderPrametersProvider(ParametersProvider.Instance),
				new OrganizationParametersProvider(ParametersProvider.Instance)
			);
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