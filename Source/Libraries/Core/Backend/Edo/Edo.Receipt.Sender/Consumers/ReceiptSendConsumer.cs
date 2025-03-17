using Edo.Contracts.Messages.Events;
using MassTransit;
using System.Threading.Tasks;

namespace Edo.Receipt.Sender.Consumers
{
	public class ReceiptSendConsumer : IConsumer<ReceiptReadyToSendEvent>
	{
		private readonly ReceiptSender _receiptSender;

		public ReceiptSendConsumer(ReceiptSender receiptSender)
		{
			_receiptSender = receiptSender ?? throw new System.ArgumentNullException(nameof(receiptSender));
		}

		public async Task Consume(ConsumeContext<ReceiptReadyToSendEvent> context)
		{
			await _receiptSender.HandleReceiptSendEvent(context.Message.ReceiptEdoTaskId, context.CancellationToken);
		}
	}
}

