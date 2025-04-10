using Edo.Contracts.Messages.Events;
using Edo.Documents;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class DocumentTaskCreatedErrorConsumer : IConsumer<DocumentTaskCreatedEvent>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public DocumentTaskCreatedErrorConsumer(
			DocumentEdoTaskHandler documentEdoTaskHandler
			)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<DocumentTaskCreatedEvent> context)
		{
			try
			{
				await _documentEdoTaskHandler.HandleNew(context.Message.Id, context.CancellationToken);
				//await _receiptSender.HandleReceiptSendEvent(msg.ReceiptEdoTaskId, context.CancellationToken);
			}
			catch(Exception ex)
			{
				Console.WriteLine($"Error processing DocumentTaskCreatedEvent: {ex.Message}");
				throw;
			}
		}
	}
}

