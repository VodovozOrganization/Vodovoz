using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Microsoft.Extensions.Logging;

namespace Edo.Transfer.Dispatcher.Consumers
{
	public class TransferRequestCreatedConsumer : IConsumer<TransferRequestCreatedEvent>
	{
		private readonly ILogger<TransferRequestCreatedConsumer> _logger;
		private readonly TransferEdoHandler _transferEdoHandler;

		public TransferRequestCreatedConsumer(
			ILogger<TransferRequestCreatedConsumer> logger, 
			TransferEdoHandler transferEdoHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_transferEdoHandler = transferEdoHandler ?? throw new ArgumentNullException(nameof(transferEdoHandler));
		}

		public async Task Consume(ConsumeContext<TransferRequestCreatedEvent> context)
		{
			await _transferEdoHandler.HandleNewTransfer(context.Message.TransferIterationId, context.CancellationToken);
		}
	}
}

