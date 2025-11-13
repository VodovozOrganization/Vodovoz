using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Documents.Consumers
{
	public class OrderDocumentProblemConsumer : IConsumer<OrderDocumentProblemEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public OrderDocumentProblemConsumer(DocumentEdoTaskHandler documentEdoTaskHandler)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<OrderDocumentProblemEvent> context)
		{
			await _documentEdoTaskHandler.HandleProblem(context.Message.DocumentId, context.CancellationToken);
		}
	}
}

