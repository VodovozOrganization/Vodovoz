using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using Nini.Config;
using NLog;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Parameters;
using VodovozSalesReceiptsService.DTO;

namespace VodovozSalesReceiptsService
{
	public static class ReceiptServiceStarter
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public static void StartService(IConfig serviceConfig, IConfig kassaConfig, IConfig[] cashboxesConfig)
		{
			string serviceHostName;
			string servicePort;
			string baseAddress;
			IList<CashBox> cashboxes;

			try {
				cashboxes = new List<CashBox>();
				foreach(var cashboxConfig in cashboxesConfig) {
					cashboxes.Add(new CashBox {
						Id = cashboxConfig.GetInt("cash_box_id"),
						RetailPoint = cashboxConfig.GetString("retail_point"),
						UserName = new Guid(cashboxConfig.GetString("user_name")),
						Password = cashboxConfig.GetString("password")
					});
				}
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
				new OrderParametersProvider(ParametersProvider.Instance),
				new OrganizationParametersProvider(ParametersProvider.Instance),
				cashboxes
			);
			fiscalizationWorker.Start();
			
			logger.Info("Служба фискализации запущена");

			var salesReceiptsInstanceProvider = new SalesReceiptsInstanceProvider(
				new BaseParametersProvider(),
				OrderSingletonRepository.GetInstance(),
				new OrderParametersProvider(ParametersProvider.Instance),
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
