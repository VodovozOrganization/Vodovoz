using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Documents.Consumers
{
	public class ManualTaskCreatedConsumer : IConsumer<ManualTaskCreatedEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public ManualTaskCreatedConsumer(
			DocumentEdoTaskHandler documentEdoTaskHandler
			)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<ManualTaskCreatedEvent> context)
		{
			await _documentEdoTaskHandler.HandleNew(context.Message.Id, context.CancellationToken);
		}
	}
}

