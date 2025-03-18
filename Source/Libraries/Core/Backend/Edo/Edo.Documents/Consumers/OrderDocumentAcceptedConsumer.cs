using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Documents.Consumers
{
	public class OrderDocumentAcceptedConsumer : IConsumer<OrderDocumentAcceptedEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public OrderDocumentAcceptedConsumer(DocumentEdoTaskHandler documentEdoTaskHandler)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<OrderDocumentAcceptedEvent> context)
		{
			await _documentEdoTaskHandler.HandleAccepted(context.Message.DocumentId, context.CancellationToken);
		}
	}
}

