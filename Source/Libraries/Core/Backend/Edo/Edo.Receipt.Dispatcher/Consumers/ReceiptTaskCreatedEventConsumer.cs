using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Edo.Receipt.Dispatcher.Consumers
{
	public class ReceiptTaskCreatedEventConsumer : IConsumer<ReceiptTaskCreatedEvent>
	{
		private readonly ILogger<ReceiptTaskCreatedEventConsumer> _logger;
		private readonly ReceiptEdoTaskHandler _receiptEdoTaskHandler;

		public ReceiptTaskCreatedEventConsumer(
			ILogger<ReceiptTaskCreatedEventConsumer> logger,
			ReceiptEdoTaskHandler receiptEdoTaskHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_receiptEdoTaskHandler = receiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(receiptEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<ReceiptTaskCreatedEvent> context)
		{
			await _receiptEdoTaskHandler.HandleNew(context.Message.ReceiptEdoTaskId, context.CancellationToken);
		}
	}
}
