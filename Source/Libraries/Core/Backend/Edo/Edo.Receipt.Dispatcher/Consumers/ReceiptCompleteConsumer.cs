using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.Consumers
{
	public class ReceiptCompleteConsumer : IConsumer<ReceiptCompleteEvent>
	{
		private readonly ILogger<ReceiptTaskCreatedConsumer> _logger;
		private readonly ReceiptEdoTaskHandler _receiptEdoTaskHandler;

		public ReceiptCompleteConsumer(
			ILogger<ReceiptTaskCreatedConsumer> logger,
			ReceiptEdoTaskHandler receiptEdoTaskHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_receiptEdoTaskHandler = receiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(receiptEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<ReceiptCompleteEvent> context)
		{
			await _receiptEdoTaskHandler.HandleCompleted(context.Message.ReceiptEdoTaskId, context.CancellationToken);
		}
	}
}
