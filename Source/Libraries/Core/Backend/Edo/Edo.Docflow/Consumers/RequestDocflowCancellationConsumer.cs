using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Docflow.Consumers
{
	public class RequestDocflowCancellationConsumer : IConsumer<RequestDocflowCancellationEvent>
	{
		private readonly DocflowCancellationHandler _docflowHandler;

		public RequestDocflowCancellationConsumer(DocflowCancellationHandler docflowHandler)
		{
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<RequestDocflowCancellationEvent> context)
		{
			await _docflowHandler.HandleDocflowCancellation(
				context.Message.TaskId, 
				context.Message.Reason,
				context.CancellationToken
			);
		}
	}
}
