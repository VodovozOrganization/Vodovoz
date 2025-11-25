using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.EquipmentTransfer.Consumers.Definitions
{
	/// <summary>
	/// Настройка MassTransit для события создания задачи по акту приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferTaskCreatedConsumerDefinition : ConsumerDefinition<EquipmentTransferTaskCreatedConsumer>
	{
		public EquipmentTransferTaskCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.equipment-transfer-task-created.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EquipmentTransferTaskCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<InformalOrderDocumenTaskCreatedEvent>();
			}
		}
	}
}
