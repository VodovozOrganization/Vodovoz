using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Microsoft.Extensions.Configuration;
using NLog;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Parameters;
using VodovozSalesReceiptsService.DTO;

namespace VodovozSalesReceiptsService
{
	public static class ReceiptServiceStarter
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static void StartService(IConfigurationSection serviceConfig, IConfigurationSection kassaConfig, IEnumerable<CashBox> cashboxes)
		{
			string serviceHostName;
			string servicePort;
			string baseAddress;

			try {
				serviceHostName = serviceConfig["service_host_name"];
				servicePort = serviceConfig["service_port"];
				
				baseAddress = kassaConfig["base_address"];
			}
			catch(Exception ex) {
				logger.Fatal(ex, "Ошибка чтения конфигурационного файла.");
				return;
			}

			logger.Info("Запуск службы фискализации и печати кассовых чеков...");

			var fiscalizationWorker = new FiscalizationWorker(
				OrderSingletonRepository.GetInstance(),
				new SalesReceiptSender(baseAddress),
				new OrderParametersProvider(SingletonParametersProvider.Instance),
				new OrganizationParametersProvider(SingletonParametersProvider.Instance),
				cashboxes
			);
			fiscalizationWorker.Start();
			
			logger.Info("Служба фискализации запущена");

			var salesReceiptsInstanceProvider = new SalesReceiptsInstanceProvider(
				new BaseParametersProvider(),
				OrderSingletonRepository.GetInstance(),
				new OrderParametersProvider(SingletonParametersProvider.Instance),
				new OrganizationParametersProvider(SingletonParametersProvider.Instance)
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
