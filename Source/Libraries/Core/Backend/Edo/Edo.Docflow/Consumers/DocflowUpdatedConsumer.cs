using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Docflow.Consumers
{
	public class DocflowUpdatedConsumer : IConsumer<EdoDocflowUpdatedEvent>
	{
		private readonly DocflowHandler _docflowHandler;

		public DocflowUpdatedConsumer(DocflowHandler docflowHandler)
		{
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<EdoDocflowUpdatedEvent> context)
		{
			await _docflowHandler.HandleDocflowUpdated(context.Message, context.CancellationToken);
		}
	}
}
