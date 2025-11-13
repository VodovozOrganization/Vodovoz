using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Transfer.Dispatcher.Consumers.Definitions
{
	public class TransferDocumentAcceptedConsumerDefinition : ConsumerDefinition<TransferDocumentAcceptedConsumer>
	{
		public TransferDocumentAcceptedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.transfer-document-accepted.consumer.transfer-dispatcher");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferDocumentAcceptedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<TransferDocumentAcceptedEvent>();
			}
		}
	}
}
