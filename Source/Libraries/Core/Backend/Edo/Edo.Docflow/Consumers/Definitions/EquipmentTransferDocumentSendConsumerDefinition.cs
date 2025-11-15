using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Definitions
{
	public class EquipmentTransferDocumentSendConsumerDefinition : ConsumerDefinition<EquipmentTransferDocumentSendConsumer>
	{
		public EquipmentTransferDocumentSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.equipment-transfer-document-send.consumer.docflow");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EquipmentTransferDocumentSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<EquipmentTransferDocumentSendEvent>();
			}
		}
	}
}
