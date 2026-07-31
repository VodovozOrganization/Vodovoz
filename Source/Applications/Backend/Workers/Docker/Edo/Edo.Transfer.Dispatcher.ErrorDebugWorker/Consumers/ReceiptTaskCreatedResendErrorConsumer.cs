using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.Consumers
{
	public class ReceiptTaskCreatedResendErrorConsumer : IConsumer<ReceiptTaskCreatedEvent>
	{
		private readonly ILogger<ReceiptTaskCreatedErrorConsumer> _logger;

		public ReceiptTaskCreatedResendErrorConsumer(
			ILogger<ReceiptTaskCreatedErrorConsumer> logger
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<ReceiptTaskCreatedEvent> context)
		{
			try
			{
				await context.Publish(context.Message, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}
