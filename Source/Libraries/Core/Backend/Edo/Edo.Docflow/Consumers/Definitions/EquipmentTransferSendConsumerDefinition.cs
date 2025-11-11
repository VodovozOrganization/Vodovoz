using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Definitions
{
	public class EquipmentTransferSendConsumerDefinition : ConsumerDefinition<EquipmentTransferSendConsumer>
	{
		public EquipmentTransferSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.equipment-transfer-send.consumer.docflow");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EquipmentTransferSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OrderDocumentSendEvent>();
			}
		}
	}
}
