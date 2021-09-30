using System;
using System.Linq;
using NLog;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services;

namespace VodovozSalesReceiptsService
{
	public class SalesReceiptsService : ISalesReceiptsService
	{
		private readonly ISalesReceiptsServiceSettings _salesReceiptsServiceSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

		public SalesReceiptsService(
			ISalesReceiptsServiceSettings salesReceiptsServiceSettings,
			IOrderRepository orderRepository,
			IOrderParametersProvider orderParametersProvider
		)
		{
			_salesReceiptsServiceSettings =
				salesReceiptsServiceSettings ?? throw new ArgumentNullException(nameof(salesReceiptsServiceSettings));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
		}

		public bool ServiceStatus()
		{
			_logger.Info("Запрос статуса службы отправки кассовых чеков");
			try
			{
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					var ordersAndReceiptNodes = _orderRepository
						.GetOrdersForCashReceiptServiceToSend(uow, _orderParametersProvider, DateTime.Today.AddDays(-3)).ToList();

					var receiptsToSend = ordersAndReceiptNodes.Count(r => r.ReceiptId == null || r.WasSent.HasValue && !r.WasSent.Value);
					_logger.Info($"Количество чеков на отправку: {receiptsToSend}");

					var activeUoWCount = UowWatcher.GetActiveUoWCount();
					_logger.Info($"Количество активных UoW: {activeUoWCount}");

					return receiptsToSend <= _salesReceiptsServiceSettings.MaxUnsendedCashReceiptsForWorkingService
						&& activeUoWCount <= _salesReceiptsServiceSettings.MaxUoWAllowed;
				}
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Ошибка при проверке работоспособности службы отправки кассовых чеков");
				return false;
			}
		}
	}
}
