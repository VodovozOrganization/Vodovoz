using Edo.Contracts.Messages.Events;
using Edo.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class DocumentTransferCompleteErrorConsumer : IConsumer<TransferCompleteEvent>
	{
		private readonly ILogger<ReceiptTransferCompleteErrorConsumer> _logger;
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public DocumentTransferCompleteErrorConsumer(
			ILogger<ReceiptTransferCompleteErrorConsumer> logger,
			DocumentEdoTaskHandler documentEdoTaskHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<TransferCompleteEvent> context)
		{
			try
			{
				await _documentEdoTaskHandler.HandleTransfered(context.Message.TransferIterationId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}

