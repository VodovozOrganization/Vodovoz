using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Transfer.Sender.Consumers
{
	public class TransferTaskPrepareToSendEventConsumer : IConsumer<TransferTaskPrepareToSendEvent>
	{
		private readonly ILogger<TransferTaskReadyToSendConsumer> _logger;
		private readonly TransferSendPreparer _transferSendPreparer;

		public TransferTaskPrepareToSendEventConsumer(
			ILogger<TransferTaskReadyToSendConsumer> logger,
			TransferSendPreparer transferSendPreparer
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_transferSendPreparer = transferSendPreparer ?? throw new ArgumentNullException(nameof(transferSendPreparer));
		}

		public async Task Consume(ConsumeContext<TransferTaskPrepareToSendEvent> context)
		{
			await _transferSendPreparer.PrepareSendAsync(context.Message.TransferTaskId, context.CancellationToken);
		}
	}
}

