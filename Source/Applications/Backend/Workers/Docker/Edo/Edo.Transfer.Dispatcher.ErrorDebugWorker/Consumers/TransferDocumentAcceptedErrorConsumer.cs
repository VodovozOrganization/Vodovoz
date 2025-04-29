using Edo.Contracts.Messages.Events;
using Edo.Documents;
using Edo.Transfer.Dispatcher;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class TransferDocumentAcceptedErrorConsumer : IConsumer<TransferDocumentAcceptedEvent>
	{
		private readonly ILogger<TransferDocumentAcceptedErrorConsumer> _logger;
		private readonly TransferEdoHandler _transferEdoHandler;

		public TransferDocumentAcceptedErrorConsumer(
			ILogger<TransferDocumentAcceptedErrorConsumer> logger,
			TransferEdoHandler transferEdoHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_transferEdoHandler = transferEdoHandler ?? throw new ArgumentNullException(nameof(transferEdoHandler));
		}

		public async Task Consume(ConsumeContext<TransferDocumentAcceptedEvent> context)
		{
			try
			{
				await _transferEdoHandler.HandleTransferDocumentAcceptance(context.Message.DocumentId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}

