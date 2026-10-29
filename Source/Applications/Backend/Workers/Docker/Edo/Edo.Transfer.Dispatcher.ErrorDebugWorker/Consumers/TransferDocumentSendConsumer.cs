using Edo.Contracts.Messages.Events;
using Edo.Docflow;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.ErrorDebugWorker.Consumers
{
	public class TransferDocumentSendErrorConsumer : IConsumer<TransferDocumentSendEvent>
	{
		private readonly DocflowSendHandler _docflowHandler;

		public TransferDocumentSendErrorConsumer(
			DocflowSendHandler docflowHandler
			)
		{
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<TransferDocumentSendEvent> context)
		{
			await _docflowHandler.SendTransferDocument(context.Message.TransferDocumentId, context.CancellationToken);
		}
	}
}
