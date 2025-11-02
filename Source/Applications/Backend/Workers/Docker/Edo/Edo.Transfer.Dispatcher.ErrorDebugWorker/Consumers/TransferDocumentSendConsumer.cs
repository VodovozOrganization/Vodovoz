using Edo.Contracts.Messages.Events;
using Edo.Docflow;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.ErrorDebugWorker.Consumers
{
	public class TransferDocumentSendErrorConsumer : IConsumer<TransferDocumentSendEvent>
	{
		private readonly ILogger<TransferDocumentSendErrorConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public TransferDocumentSendErrorConsumer(
			ILogger<TransferDocumentSendErrorConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<TransferDocumentSendEvent> context)
		{
			try
			{
				await _docflowHandler.HandleTransferDocument(context.Message.TransferDocumentId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}
