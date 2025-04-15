using Edo.Contracts.Messages.Events;
using Edo.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.ErrorDebugWorker.Consumers
{
	public class OrderDocumentAcceptedErrorConsumer : IConsumer<OrderDocumentAcceptedEvent>
	{
		private readonly ILogger<OrderDocumentAcceptedErrorConsumer> _logger;
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public OrderDocumentAcceptedErrorConsumer(
			ILogger<OrderDocumentAcceptedErrorConsumer> logger,
			DocumentEdoTaskHandler documentEdoTaskHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<OrderDocumentAcceptedEvent> context)
		{
			try
			{
				await _documentEdoTaskHandler.HandleAccepted(context.Message.DocumentId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}

