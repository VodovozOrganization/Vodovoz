using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
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
			try
			{
				await _receiptEdoTaskHandler.HandleNew(context.Message.ReceiptEdoTaskId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error while processing EdoRequestCreatedEvent");
				await context.ConsumeCompleted;
			}
		}
	}
}
