using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Docflow.Consumers
{
	public class RequestDocflowCancellationConsumer : IConsumer<RequestDocflowCancellationEvent>
	{
		private readonly ILogger<RequestDocflowCancellationConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public RequestDocflowCancellationConsumer(
			ILogger<RequestDocflowCancellationConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<RequestDocflowCancellationEvent> context)
		{
			await _docflowHandler.HandleTransferDocumentCancellation(
				context.Message.TaskId, 
				context.Message.Reason,
				context.CancellationToken
				);
		}
	}
}
