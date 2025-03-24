using Edo.Contracts.Messages.Events;
using Edo.Transfer.Dispatcher;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class TransferDocumentAcceptedErrorConsumer : IConsumer<Batch<TransferDocumentAcceptedEvent>>
	{
		private readonly TransferEdoHandler _transferEdoHandler;

		public TransferDocumentAcceptedErrorConsumer(
			TransferEdoHandler transferEdoHandler)
		{
			_transferEdoHandler = transferEdoHandler ?? throw new ArgumentNullException(nameof(transferEdoHandler));
		}

		public async Task Consume(ConsumeContext<Batch<TransferDocumentAcceptedEvent>> context)
		{
			foreach(var batchItem in context.Message)
			{
				var msg = batchItem.Message;

				await _transferEdoHandler.HandleTransferDocumentAcceptance(msg.DocumentId, context.CancellationToken);
			}
		}
	}
}

