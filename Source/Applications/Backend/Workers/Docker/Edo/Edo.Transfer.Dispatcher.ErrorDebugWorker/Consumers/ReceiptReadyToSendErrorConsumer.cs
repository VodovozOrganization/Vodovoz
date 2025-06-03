using Edo.Contracts.Messages.Events;
using Edo.Receipt.Sender;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class ReceiptReadyToSendErrorConsumer : IConsumer<ReceiptReadyToSendEvent>
	{
		private readonly ILogger<ReceiptReadyToSendErrorConsumer> _logger;
		private readonly ReceiptSender _receiptSender;
		private readonly ReceiptEdoTaskHandler _receiptEdoTaskHandler;

		public ReceiptReadyToSendErrorConsumer(
			ILogger<ReceiptReadyToSendErrorConsumer> logger,
			ReceiptSender receiptSender,
			ReceiptEdoTaskHandler receiptEdoTaskHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_receiptSender = receiptSender ?? throw new ArgumentNullException(nameof(receiptSender));
			_receiptEdoTaskHandler = receiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(receiptEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<ReceiptReadyToSendEvent> context)
		{
			try
			{
				await _receiptSender.HandleReceiptSendEvent(context.Message.ReceiptEdoTaskId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}

