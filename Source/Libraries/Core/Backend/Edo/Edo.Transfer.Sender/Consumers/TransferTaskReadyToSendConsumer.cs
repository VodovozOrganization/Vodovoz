using Edo.Transport.Messages.Events;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Edo.Transfer.Sender.Consumers
{
	public class TransferTaskReadyToSendConsumer : IConsumer<TransferTaskReadyToSendEvent>
	{
		private readonly TransferSendHandler _transferSendHandler;

		public TransferTaskReadyToSendConsumer(TransferSendHandler transferSendHandler)
		{
			_transferSendHandler = transferSendHandler ?? throw new ArgumentNullException(nameof(transferSendHandler));
		}

		public async Task Consume(ConsumeContext<TransferTaskReadyToSendEvent> context)
		{
			await _transferSendHandler.HandleReadyToSend(context.Message.Id, context.CancellationToken);
		}
	}
}

