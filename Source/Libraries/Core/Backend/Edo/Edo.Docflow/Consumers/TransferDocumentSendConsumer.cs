using Edo.Contracts.Messages.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Docflow.Consumers
{
	public class TransferDocumentSendConsumer : IConsumer<TransferDocumentSendEvent>
	{
		private readonly DocflowSendHandler _docflowHandler;

		public TransferDocumentSendConsumer(DocflowSendHandler docflowHandler)
		{
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<TransferDocumentSendEvent> context)
		{
			await _docflowHandler.SendTransferDocument(context.Message.TransferDocumentId, context.CancellationToken);
		}
	}
}
