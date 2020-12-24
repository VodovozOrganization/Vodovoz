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
		private readonly IOrderPrametersProvider orderPrametersProvider;
		private readonly IOrganizationParametersProvider organizationParametersProvider;
		private readonly ILogger logger = LogManager.GetCurrentClassLogger();

		public SalesReceiptsService(
			ISalesReceiptsServiceSettings salesReceiptsServiceSettings,
			IOrderRepository orderRepository,
			IOrderPrametersProvider orderPrametersProvider,
			IOrganizationParametersProvider organizationParametersProvider
			)
		{
			this.salesReceiptsServiceSettings = salesReceiptsServiceSettings ?? throw new ArgumentNullException(nameof(salesReceiptsServiceSettings));
			this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			this.orderPrametersProvider = orderPrametersProvider ?? throw new ArgumentNullException(nameof(orderPrametersProvider));
			this.organizationParametersProvider = organizationParametersProvider ?? throw new ArgumentNullException(nameof(organizationParametersProvider));
		}

		public bool ServiceStatus()
		{
			logger.Info("Запрос статуса службы отправки кассовых чеков");
			try {
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					var ordersAndReceiptNodes = orderRepository
						.GetOrdersForCashReceiptServiceToSend(uow, orderPrametersProvider, organizationParametersProvider, DateTime.Today.AddDays(-3)).ToList();

					var receiptsToSend = ordersAndReceiptNodes.Count(r => r.ReceiptId == null || r.WasSent.HasValue && !r.WasSent.Value);
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
