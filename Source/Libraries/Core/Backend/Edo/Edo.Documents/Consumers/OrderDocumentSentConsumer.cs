using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Documents.Consumers
{
	public class OrderDocumentSentConsumer : IConsumer<OrderDocumentSentEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public OrderDocumentSentConsumer(DocumentEdoTaskHandler documentEdoTaskHandler)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<OrderDocumentSentEvent> context)
		{
			await _documentEdoTaskHandler.HandleSent(context.Message.Id, context.CancellationToken);
		}
	}

	
}

