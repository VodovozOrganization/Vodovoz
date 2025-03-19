using Edo.Contracts.Messages.Events;
using Edo.Receipt.Sender;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Receipt.Dispatcher.ErrorDebug.Consumers
{
	public class ReceiptReadyToSendErrorConsumer : IConsumer<Batch<ReceiptReadyToSendEvent>>
	{
		private readonly ReceiptSender _receiptSender;
		private readonly ReceiptEdoTaskHandler _receiptEdoTaskHandler;

		public ReceiptReadyToSendErrorConsumer(
			ReceiptSender receiptSender,
			ReceiptEdoTaskHandler receiptEdoTaskHandler
			)
		{
			_receiptSender = receiptSender ?? throw new ArgumentNullException(nameof(receiptSender));
			_receiptEdoTaskHandler = receiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(receiptEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<Batch<ReceiptReadyToSendEvent>> context)
		{
			foreach(var batchItem in context.Message)
			{
				var msg = batchItem.Message;
				try
				{
					await _receiptEdoTaskHandler.HandleNew(msg.ReceiptEdoTaskId, context.CancellationToken);
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

