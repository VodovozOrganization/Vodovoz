using Edo.Contracts.Messages.Events;
using Edo.Docflow;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.ErrorDebugWorker.Consumers
{
	public class OrderDocumentSendErrorConsumer : IConsumer<OrderDocumentSendEvent>
	{
		private readonly DocflowSendHandler _docflowHandler;

		public OrderDocumentSendErrorConsumer(DocflowSendHandler docflowHandler)
		{
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<OrderDocumentSendEvent> context)
		{
			await _docflowHandler.SendOrderDocument(context.Message.OrderDocumentId, context.CancellationToken);
		}
	}
}
