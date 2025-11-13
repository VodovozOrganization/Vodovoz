using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Transfer.Dispatcher.Consumers
{
	public class TransferDocumentProblemConsumer : IConsumer<TransferDocumentProblemEvent>
	{
		private readonly TransferEdoHandler _transferEdoHandler;

		public TransferDocumentProblemConsumer(TransferEdoHandler transferEdoHandler)
		{
			_transferEdoHandler = transferEdoHandler ?? throw new ArgumentNullException(nameof(transferEdoHandler));
		}

		public async Task Consume(ConsumeContext<TransferDocumentProblemEvent> context)
		{
			await _transferEdoHandler.HandleTransferDocumentProblem(context.Message.DocumentId, context.CancellationToken);
		}
	}
}

