using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Definitions
{
	public class TransferDocumentSendConsumerDefinition : ConsumerDefinition<TransferDocumentSendConsumer>
	{
		public TransferDocumentSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-document-send.consumer.docflow");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferDocumentSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferDocumentSendEvent>();
			}
		}
	}
}
