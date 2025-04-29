using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Edo.Documents.Consumers
{
	public class DocumentTaskCreatedConsumer : IConsumer<DocumentTaskCreatedEvent>
	{
		private readonly ILogger<DocumentTaskCreatedConsumer> _logger;
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public DocumentTaskCreatedConsumer(
			ILogger<DocumentTaskCreatedConsumer> logger, 
			DocumentEdoTaskHandler documentEdoTaskHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<DocumentTaskCreatedEvent> context)
		{
			await _documentEdoTaskHandler.HandleNew(context.Message.Id, context.CancellationToken);
		}
	}
}

