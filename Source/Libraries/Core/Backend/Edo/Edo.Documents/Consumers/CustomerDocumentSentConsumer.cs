using Edo.Transport.Messages.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Documents.Consumers
{
	public class CustomerDocumentSentConsumer : IConsumer<CustomerDocumentSentEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public CustomerDocumentSentConsumer(DocumentEdoTaskHandler documentEdoTaskHandler)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<CustomerDocumentSentEvent> context)
		{
			await _documentEdoTaskHandler.HandleSent(context.Message.Id, context.CancellationToken);
		}
	}

	
}

