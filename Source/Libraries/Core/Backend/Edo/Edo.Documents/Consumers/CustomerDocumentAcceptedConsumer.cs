using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Documents.Consumers
{
	public class CustomerDocumentAcceptedConsumer : IConsumer<CustomerDocumentAcceptedEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public CustomerDocumentAcceptedConsumer(DocumentEdoTaskHandler documentEdoTaskHandler)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<CustomerDocumentAcceptedEvent> context)
		{
			await _documentEdoTaskHandler.HandleAccepted(context.Message.DocumentId, context.CancellationToken);
		}
	}
}

