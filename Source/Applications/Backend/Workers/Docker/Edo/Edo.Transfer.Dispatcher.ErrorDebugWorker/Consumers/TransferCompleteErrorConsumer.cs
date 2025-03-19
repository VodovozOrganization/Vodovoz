using Edo.Contracts.Messages.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class TransferCompleteErrorConsumer : IConsumer<Batch<TransferCompleteEvent>>
	{
		private readonly ReceiptEdoTaskHandler _receiptEdoTaskHandler;

		public TransferCompleteErrorConsumer(ReceiptEdoTaskHandler receiptEdoTaskHandler)
		{
			_receiptEdoTaskHandler = receiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(receiptEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<Batch<TransferCompleteEvent>> context)
		{
			foreach(var batchItem in context.Message)
			{
				var msg = batchItem.Message;

				await _receiptEdoTaskHandler.HandleTransfered(msg.TransferIterationId, context.CancellationToken);
			}
		}
	}
}

