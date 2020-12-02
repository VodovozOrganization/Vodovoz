using System;
using System.Linq;
using NLog;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;

namespace VodovozSalesReceiptsService
{
	public class SalesReceiptsService : ISalesReceiptsService
	{
		private readonly ISalesReceiptsServiceSettings salesReceiptsServiceSettings;
		private readonly IOrderRepository orderRepository;
		private readonly ILogger logger = LogManager.GetCurrentClassLogger();

		public SalesReceiptsService(ISalesReceiptsServiceSettings salesReceiptsServiceSettings, IOrderRepository orderRepository)
		{
			this.salesReceiptsServiceSettings = salesReceiptsServiceSettings ?? throw new ArgumentNullException(nameof(salesReceiptsServiceSettings));
			this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}

		public bool ServiceStatus()
		{
			logger.Info("Запрос статуса службы отправки кассовых чеков");
			try {
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var ordersAndReceiptNodes = orderRepository
						.GetOrdersForCashReceiptServiceToSend(uow, DateTime.Today.AddDays(-3)).ToList();
					
					var withoutReceipts = ordersAndReceiptNodes.Where(r => r.ReceiptId == null);
					var withNotSentReceipts = ordersAndReceiptNodes.Where(r => r.ReceiptId.HasValue && r.WasSent != true);
					
					var receiptsToSend = withoutReceipts.Count() + withNotSentReceipts.Count();
					logger.Info($"Количество чеков на отправку: {receiptsToSend}");
					return receiptsToSend <= salesReceiptsServiceSettings.MaxUnsendedCashReceiptsForWorkingService;
				}
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при проверке работоспособности службы отправки кассовых чеков");
				return false;
			}
		}
		
	}
}
