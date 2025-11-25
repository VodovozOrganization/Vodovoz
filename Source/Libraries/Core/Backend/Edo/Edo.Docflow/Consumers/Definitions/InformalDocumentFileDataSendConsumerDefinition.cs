using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.InformalOrderDocuments.Consumers.Definitions
{
	public class InformalDocumentFileDataSendConsumerDefinition : ConsumerDefinition<InformalDocumentFileDataSendConsumer>
	{
		public InformalDocumentFileDataSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.informal-document-file-data-send.consumer.docflow");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<InformalDocumentFileDataSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<InformalDocumentFileDataSendEvent>();
			}
		}
	}
}
