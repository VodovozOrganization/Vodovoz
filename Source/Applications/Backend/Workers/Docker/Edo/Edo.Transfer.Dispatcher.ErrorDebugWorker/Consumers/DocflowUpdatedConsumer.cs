using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Docflow.Consumers
{
	public class DocflowUpdatedErrorConsumer : IConsumer<EdoDocflowUpdatedEvent>
	{
		private readonly ILogger<DocflowUpdatedConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public DocflowUpdatedErrorConsumer(
			ILogger<DocflowUpdatedConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<EdoDocflowUpdatedEvent> context)
		{
			try
			{
				await _docflowHandler.HandleDocflowUpdated(context.Message, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}
