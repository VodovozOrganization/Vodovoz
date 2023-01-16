using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using NLog;
using Vodovoz.Core.DataService;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Parameters;
using VodovozSalesReceiptsService.DTO;

namespace VodovozSalesReceiptsService
{
	public static class ReceiptServiceStarter
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public static void StartService(string serviceHostName, string servicePort, string modulKassaBaseAddress,
			IEnumerable<CashBox> cashboxes)
		{
			_logger.Info("Запуск службы фискализации и печати кассовых чеков...");

			var parametersProvider = new ParametersProvider();

			var fiscalizationWorker = new FiscalizationWorker(
				new OrderRepository(),
				new SalesReceiptSender(modulKassaBaseAddress),
				new OrderParametersProvider(parametersProvider),
				cashboxes
			);
			fiscalizationWorker.Start();

			_logger.Info("Служба фискализации запущена");

			/*var salesReceiptsInstanceProvider = new SalesReceiptsInstanceProvider(
				new BaseParametersProvider(parametersProvider),
				new OrderRepository(),
				new OrderParametersProvider(parametersProvider)
			);
			WebServiceHost salesReceiptsHost = new SalesReceiptsServiceHost(salesReceiptsInstanceProvider);
			salesReceiptsHost.AddServiceEndpoint(
				typeof(ISalesReceiptsService),
				new WebHttpBinding(),
				$"http://{serviceHostName}:{servicePort}/SalesReceipts"
			);
			salesReceiptsHost.Open();
			_logger.Info("Запущена служба мониторинга отправки чеков");*/
		}
	}
}
