using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Edo.Docflow.Consumers
{
	public class OrderDocumentSendConsumer : IConsumer<OrderDocumentSendEvent>
	{
		private readonly DocflowSendHandler _docflowHandler;

		public OrderDocumentSendConsumer(DocflowSendHandler docflowHandler)
		{
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<OrderDocumentSendEvent> context)
		{
			await _docflowHandler.SendOrderDocument(context.Message.OrderDocumentId, context.CancellationToken);
		}
	}
}
