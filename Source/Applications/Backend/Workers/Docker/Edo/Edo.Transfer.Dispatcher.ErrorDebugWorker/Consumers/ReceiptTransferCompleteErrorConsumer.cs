using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class ReceiptTransferCompleteErrorConsumer : IConsumer<TransferCompleteEvent>
	{
		private readonly ILogger<ReceiptTransferCompleteErrorConsumer> _logger;
		private readonly ReceiptEdoTaskHandler _receiptEdoTaskHandler;

		public ReceiptTransferCompleteErrorConsumer(
			ILogger<ReceiptTransferCompleteErrorConsumer> logger,
			ReceiptEdoTaskHandler receiptEdoTaskHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_receiptEdoTaskHandler = receiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(receiptEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<TransferCompleteEvent> context)
		{
			try
			{
				await _receiptEdoTaskHandler.HandleTransfered(context.Message.TransferIterationId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}

