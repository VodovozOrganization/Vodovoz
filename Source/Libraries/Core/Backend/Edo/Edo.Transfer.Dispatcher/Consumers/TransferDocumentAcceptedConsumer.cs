using Edo.Transport.Messages.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Transfer.Dispatcher.Consumers
{
	public class TransferDocumentAcceptedConsumer : IConsumer<TransferDocumentAcceptedEvent>
	{
		private readonly TransferEdoHandler _transferEdoHandler;

		public TransferDocumentAcceptedConsumer(TransferEdoHandler transferEdoHandler)
		{
			_transferEdoHandler = transferEdoHandler ?? throw new ArgumentNullException(nameof(transferEdoHandler));
		}

		public async Task Consume(ConsumeContext<TransferDocumentAcceptedEvent> context)
		{
			await _transferEdoHandler.HandleTransferDocumentAcceptance(context.Message.DocumentId, context.CancellationToken);
		}
	}
}

