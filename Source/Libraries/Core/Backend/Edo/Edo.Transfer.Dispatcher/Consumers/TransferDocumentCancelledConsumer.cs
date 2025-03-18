using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Transfer.Dispatcher.Consumers
{
	public class TransferDocumentCancelledConsumer : IConsumer<TransferDocumentCancelledEvent>
	{
		private readonly TransferEdoHandler _transferEdoHandler;

		public TransferDocumentCancelledConsumer(TransferEdoHandler transferEdoHandler)
		{
			_transferEdoHandler = transferEdoHandler ?? throw new ArgumentNullException(nameof(transferEdoHandler));
		}

		public async Task Consume(ConsumeContext<TransferDocumentCancelledEvent> context)
		{
			await _transferEdoHandler.HandleTransferDocumentCancelled(context.Message.DocumentId, context.CancellationToken);
		}
	}
}

