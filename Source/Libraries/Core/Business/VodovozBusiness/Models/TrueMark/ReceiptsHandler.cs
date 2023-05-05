using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Models.TrueMark
{
	public class ReceiptsHandler
	{
		private readonly ReceiptPreparerFactory _receiptPreparerFactory;
		private readonly OrderReceiptCreatorFactory _orderReceiptCreatorFactory;
		private readonly ICashReceiptRepository _cashReceiptRepository;

		public ReceiptsHandler(
			ReceiptPreparerFactory receiptPreparerFactory,
			OrderReceiptCreatorFactory orderReceiptCreatorFactory,
			ICashReceiptRepository cashReceiptRepository
		)
		{
			_receiptPreparerFactory = receiptPreparerFactory ?? throw new ArgumentNullException(nameof(receiptPreparerFactory));
			_orderReceiptCreatorFactory = orderReceiptCreatorFactory ?? throw new ArgumentNullException(nameof(orderReceiptCreatorFactory));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
		}

		public async Task HandleReceiptsAsync(CancellationToken cancellationToken)
		{
			var receiptIds = _cashReceiptRepository.GetReceiptIdsForPrepare(100);
			foreach(var receiptId in receiptIds)
			{
				using(var preparer = _receiptPreparerFactory.Create(receiptId))
				{
					await preparer.PrepareAsync(cancellationToken);
				}
			}
		}

		public async Task CreateSelfDeliveryReceiptsAsync(CancellationToken cancellationToken)
		{
			var orderIds = _cashReceiptRepository.GetSelfdeliveryOrderIdsForCashReceipt();
			foreach(var orderId in orderIds)
			{
				using(var creator = _orderReceiptCreatorFactory.CreateSelfDeliveryReceiptCreator(orderId))
				{
					await creator.CreateReceiptAsync(cancellationToken);
				}
			}
		}
		
		public async Task CreateDeliveryOrderReceiptsAsync(CancellationToken cancellationToken)
		{
			var orderIds = _cashReceiptRepository.GetDeliveryOrderIdsForCashReceipt();
			foreach(var orderId in orderIds)
			{
				using(var creator = _orderReceiptCreatorFactory.CreateDeliveryOrderReceiptCreator(orderId))
				{
					await creator.CreateReceiptAsync(cancellationToken);
				}
			}
		}
	}
}
