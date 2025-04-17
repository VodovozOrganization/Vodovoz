using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Edo.Docflow.Consumers
{
	public class OrderDocumentSendConsumer : IConsumer<OrderDocumentSendEvent>
	{
		private readonly ILogger<OrderDocumentSendConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public OrderDocumentSendConsumer(
			ILogger<OrderDocumentSendConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<OrderDocumentSendEvent> context)
		{
			await _docflowHandler.HandleOrderDocument(context.Message.OrderDocumentId, context.CancellationToken);
		}
	}
}
