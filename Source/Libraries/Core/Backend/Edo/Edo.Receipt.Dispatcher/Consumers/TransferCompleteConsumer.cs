using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Receipt.Dispatcher.Consumers
{
	public class TransferCompleteConsumer : IConsumer<TransferCompleteEvent>
	{
		private readonly ReceiptEdoTaskHandler _receiptEdoTaskHandler;

		public TransferCompleteConsumer(ReceiptEdoTaskHandler receiptEdoTaskHandler)
		{
			_receiptEdoTaskHandler = receiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(receiptEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<TransferCompleteEvent> context)
		{
			await _receiptEdoTaskHandler.HandleTransfered(context.Message.TransferEdoTaskId, context.CancellationToken);
		}
	}
}

