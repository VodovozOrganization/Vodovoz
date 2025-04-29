using Edo.Contracts.Messages.Events;
using Edo.Docflow;
using Edo.Docflow.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.ErrorDebugWorker.Consumers
{
	public class OrderDocumentSendErrorConsumer : IConsumer<OrderDocumentSendEvent>
	{
		private readonly ILogger<OrderDocumentSendConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public OrderDocumentSendErrorConsumer(
			ILogger<OrderDocumentSendConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<OrderDocumentSendEvent> context)
		{
			try
			{
				await _docflowHandler.HandleOrderDocument(context.Message.OrderDocumentId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}
