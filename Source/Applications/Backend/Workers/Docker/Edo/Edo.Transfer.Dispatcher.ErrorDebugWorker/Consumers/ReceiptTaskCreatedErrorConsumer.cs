using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.Consumers
{
	public class ReceiptTaskCreatedErrorConsumer : IConsumer<ReceiptTaskCreatedEvent>
	{
		private readonly ILogger<ReceiptTaskCreatedErrorConsumer> _logger;
		private readonly ReceiptEdoTaskHandler _receiptEdoTaskHandler;

		public ReceiptTaskCreatedErrorConsumer(
			ILogger<ReceiptTaskCreatedErrorConsumer> logger,
			ReceiptEdoTaskHandler receiptEdoTaskHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_receiptEdoTaskHandler = receiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(receiptEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<ReceiptTaskCreatedEvent> context)
		{
			try
			{
				await _receiptEdoTaskHandler.HandleNew(context.Message.ReceiptEdoTaskId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}
