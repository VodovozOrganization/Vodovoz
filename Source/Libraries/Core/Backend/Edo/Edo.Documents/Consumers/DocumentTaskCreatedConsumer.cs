using Edo.Transport.Messages.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Documents.Consumers
{
	public class DocumentTaskCreatedConsumer : IConsumer<DocumentTaskCreatedEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public DocumentTaskCreatedConsumer(DocumentEdoTaskHandler documentEdoTaskHandler)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<DocumentTaskCreatedEvent> context)
		{
			await _documentEdoTaskHandler.HandleNew(context.Message.Id, context.CancellationToken);
		}
	}
}

