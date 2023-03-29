using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Models.TrueMark
{
	public class ReceiptsHandler
	{
		private readonly ReceiptPreparerFactory _receiptPreparerFactory;
		private readonly SelfdeliveryReceiptCreatorFactory _selfdeliveryReceiptCreatorFactory;
		private readonly ICashReceiptRepository _cashReceiptRepository;

		public ReceiptsHandler(
			ReceiptPreparerFactory receiptPreparerFactory,
			SelfdeliveryReceiptCreatorFactory selfdeliveryReceiptCreatorFactory,
			ICashReceiptRepository cashReceiptRepository
		)
		{
			_receiptPreparerFactory = receiptPreparerFactory ?? throw new ArgumentNullException(nameof(receiptPreparerFactory));
			_selfdeliveryReceiptCreatorFactory = selfdeliveryReceiptCreatorFactory ?? throw new ArgumentNullException(nameof(selfdeliveryReceiptCreatorFactory));
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

		public async Task CreateSelfdeliveryReceiptsAsync(CancellationToken cancellationToken)
		{
			var orderIds = _cashReceiptRepository.GetSelfdeliveryOrderIdsForCashReceipt();
			foreach(var orderId in orderIds)
			{
				using(var creator = _selfdeliveryReceiptCreatorFactory.Create(orderId))
				{
					await creator.CreateReceiptAsync(cancellationToken);
				}
			}
		}
	}
}
