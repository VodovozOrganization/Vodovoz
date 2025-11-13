using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Documents.Consumers
{
	public class OrderDocumentCancelledConsumer : IConsumer<OrderDocumentCancelledEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public OrderDocumentCancelledConsumer(DocumentEdoTaskHandler documentEdoTaskHandler)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<OrderDocumentCancelledEvent> context)
		{
			await _documentEdoTaskHandler.HandleCancelled(context.Message.DocumentId, context.CancellationToken);
		}
	}
}

