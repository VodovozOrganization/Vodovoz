using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Docflow.Consumers
{
	public class TransferDocumentSendConsumer : IConsumer<TransferDocumentSendEvent>
	{
		private readonly DocflowHandler _docflowHandler;

		public TransferDocumentSendConsumer(DocflowHandler docflowHandler)
		{
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<TransferDocumentSendEvent> context)
		{
			await _docflowHandler.HandleTransferDocument(context.Message.Id, context.CancellationToken);
		}
	}
}
