using Edo.Contracts.Messages.Events;
using Edo.Documents;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Edo.EquipmentTransfer.Consumers
{
	/// <summary>
	/// Настройка MassTransit для обработки созданной задачи по акту приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferTaskCreatedConsumer : IConsumer<EquipmentTransferTaskCreatedEvent>
	{
		private readonly ILogger<EquipmentTransferTaskCreatedConsumer> _logger;
		private readonly EquipmentTransferEdoTaskHandler _equipmentTransferEdoTaskHandler;

		public EquipmentTransferTaskCreatedConsumer(
			ILogger<EquipmentTransferTaskCreatedConsumer> logger,
			EquipmentTransferEdoTaskHandler equipmentTransferEdoTaskHandler
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_equipmentTransferEdoTaskHandler = equipmentTransferEdoTaskHandler ?? throw new ArgumentNullException(nameof(equipmentTransferEdoTaskHandler));
		}

		public async Task Consume(ConsumeContext<EquipmentTransferTaskCreatedEvent> context)
		{
			await _equipmentTransferEdoTaskHandler.SendTransferDocument(context.Message.EquipmentTransferTaskId, context.CancellationToken);
		}
	}
}
