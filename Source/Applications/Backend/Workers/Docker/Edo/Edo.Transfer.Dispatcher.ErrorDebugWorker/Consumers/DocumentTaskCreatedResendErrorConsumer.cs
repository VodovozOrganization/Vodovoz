using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class DocumentTaskCreatedResendErrorConsumer : IConsumer<DocumentTaskCreatedEvent>
	{
		private readonly ILogger<DocumentTaskCreatedResendErrorConsumer> _logger;

		public DocumentTaskCreatedResendErrorConsumer(
			ILogger<DocumentTaskCreatedResendErrorConsumer> logger
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Consume(ConsumeContext<DocumentTaskCreatedEvent> context)
		{
			try
			{
				var message = context.Message;
				await context.Publish(message, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}

