using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Edo.Receipt.Dispatcher.Consumers
{
	public class ReceiptCompleteEventConsumer : IConsumer<ReceiptCompleteEvent>
	{
		private readonly ILogger<ReceiptTaskCreatedEventConsumer> _logger;

		public ReceiptCompleteEventConsumer(
			ILogger<ReceiptTaskCreatedEventConsumer> logger
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<ReceiptCompleteEvent> context)
		{
			try
			{
				await _taskScheduler.CreateTask(context.Message.EdoTaskId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error while processing EdoRequestCreatedEvent");
				await Task.CompletedTask;
			}
		}
	}
}
