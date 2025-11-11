using Edo.Contracts.Messages.Events;
using Edo.Docflow;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.EquipmentTransfer.Consumers
{
	/// <summary>
	/// Настройка MassTransit для события создания задачи по акту приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferTaskCreatedConsumer : IConsumer<EquipmentTransferTaskCreatedEvent>
	{
		private readonly ILogger<EquipmentTransferTaskCreatedConsumer> _logger;
		private readonly DocflowHandler _docflowHandler;

		public EquipmentTransferTaskCreatedConsumer(
			ILogger<EquipmentTransferTaskCreatedConsumer> logger,
			DocflowHandler docflowHandler
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_docflowHandler = docflowHandler ?? throw new ArgumentNullException(nameof(docflowHandler));
		}
		
		public async Task Consume(ConsumeContext<EquipmentTransferTaskCreatedEvent> context)
		{
			await _docflowHandler.HandleEquipmentTransfer(context.Message.EquipmentTransferTaskId, context.CancellationToken);
		}
	}
}
