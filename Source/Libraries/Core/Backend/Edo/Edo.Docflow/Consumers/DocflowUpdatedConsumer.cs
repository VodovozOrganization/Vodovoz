using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Edo.Docflow.Consumers
{
	public class DocflowUpdatedConsumer : IConsumer<EdoDocflowUpdatedEvent>
	{
		private readonly ILogger<DocflowUpdatedConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public DocflowUpdatedConsumer(
			ILogger<DocflowUpdatedConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<EdoDocflowUpdatedEvent> context)
		{
			await _docflowHandler.HandleDocflowUpdated(context.Message, context.CancellationToken);
		}
	}
}
