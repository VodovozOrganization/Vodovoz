using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Transfer.Dispatcher.Consumers
{
	public class TransferRequestCreatedConsumer : IConsumer<TransferRequestCreatedEvent>
	{
		private readonly TransferEdoHandler _transferEdoHandler;

		public TransferRequestCreatedConsumer(TransferEdoHandler transferEdoHandler)
		{
			_transferEdoHandler = transferEdoHandler ?? throw new ArgumentNullException(nameof(transferEdoHandler));
		}

		public async Task Consume(ConsumeContext<TransferRequestCreatedEvent> context)
		{
			await _transferEdoHandler.HandleDocumentTask(context.Message.Id, context.CancellationToken);
		}
	}
}

