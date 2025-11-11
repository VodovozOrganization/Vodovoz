using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.Docflow.Consumers
{
	public class EquipmentTransferSendConsumer : IConsumer<EquipmentTransferSendEvent>
	{
		private readonly ILogger<EquipmentTransferSendConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public EquipmentTransferSendConsumer(
			ILogger<EquipmentTransferSendConsumer> logger,
			DocflowHandler docflowHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}

		public async Task Consume(ConsumeContext<EquipmentTransferSendEvent> context)
		{
			await _docflowHandler.HandleEquipmentTransfer(context.Message.EquipmentTransferId, context.CancellationToken);
		}
	}
}
