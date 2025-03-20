using Edo.Contracts.Messages.Events;
using Edo.Documents;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class DocumentTaskCreatedErrorConsumer : IConsumer<Batch<DocumentTaskCreatedEvent>>
	{
		private readonly DocumentEdoTaskHandler _documentEdoTaskHandler;

		public DocumentTaskCreatedErrorConsumer(
			DocumentEdoTaskHandler documentEdoTaskHandler
			)
		{
			_documentEdoTaskHandler = documentEdoTaskHandler ?? throw new ArgumentNullException(nameof(documentEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<Batch<DocumentTaskCreatedEvent>> context)
		{
			foreach(var batchItem in context.Message)
			{
				var msg = batchItem.Message;
				try
				{
					await _documentEdoTaskHandler.HandleNew(msg.Id, context.CancellationToken);
					//await _receiptSender.HandleReceiptSendEvent(msg.ReceiptEdoTaskId, context.CancellationToken);
				}
				catch(Exception ex)
				{
					throw;
				}
			}
		}
	}
}

