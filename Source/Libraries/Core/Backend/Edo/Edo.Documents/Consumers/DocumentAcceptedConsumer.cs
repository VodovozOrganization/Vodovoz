using Edo.Transport.Messages.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Documents.Consumers
{
	public class DocumentAcceptedConsumer : IConsumer<CustomerDocumentAcceptedEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public DocumentAcceptedConsumer(DocumentEdoTaskHandler documentEdoTaskHandler)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<CustomerDocumentAcceptedEvent> context)
		{
			await _documentEdoTaskHandler.HandleAccepted(context.Message.Id, context.CancellationToken);
		}
	}
}

