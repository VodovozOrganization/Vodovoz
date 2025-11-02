using Edo.Contracts.Messages.Events;
using Edo.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class DocumentTaskCreatedErrorConsumer : IConsumer<DocumentTaskCreatedEvent>
	{
		private readonly ILogger<DocumentTaskCreatedErrorConsumer> _logger;
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public DocumentTaskCreatedErrorConsumer(
			ILogger<DocumentTaskCreatedErrorConsumer> logger,
			DocumentEdoTaskHandler documentEdoTaskHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<DocumentTaskCreatedEvent> context)
		{
			try
			{
				await _documentEdoTaskHandler.HandleNew(context.Message.Id, context.CancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Error processing event");
				throw;
			}
		}
	}
}

