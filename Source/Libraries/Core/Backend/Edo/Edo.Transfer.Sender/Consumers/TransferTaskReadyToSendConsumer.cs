using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Transfer.Sender.Consumers
{
	public class TransferTaskReadyToSendConsumer : IConsumer<TransferTaskReadyToSendEvent>
	{
		private readonly ILogger<TransferTaskReadyToSendConsumer> _logger;
		private readonly TransferSender _transferSendHandler;

		public TransferTaskReadyToSendConsumer(
			ILogger<TransferTaskReadyToSendConsumer> logger,
			TransferSender transferSendHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_transferSendHandler = transferSendHandler ?? throw new ArgumentNullException(nameof(transferSendHandler));
		}

		public async Task Consume(ConsumeContext<TransferTaskReadyToSendEvent> context)
		{
			await _transferSendHandler.HandleReadyToSend(context.Message.TransferTaskId, context.CancellationToken);
		}
	}
}

