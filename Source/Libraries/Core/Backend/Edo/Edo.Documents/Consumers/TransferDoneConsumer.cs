using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Documents.Consumers
{
	public class TransferDoneConsumer : IConsumer<TransferDoneEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public TransferDoneConsumer(DocumentEdoTaskHandler documentEdoTaskHandler)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<TransferDoneEvent> context)
		{
			await _documentEdoTaskHandler.HandleTransfered(context.Message.Id, context.CancellationToken);
		}
	}
}

