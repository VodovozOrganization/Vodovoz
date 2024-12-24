using MassTransit;
using RabbitMQ.Client;

namespace Edo.Transfer.Dispatcher.Consumers.Definitions
{
	public class TransferDocumentAcceptedConsumerDefinition : ConsumerDefinition<TransferDocumentAcceptedConsumer>
	{
		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TransferDocumentAcceptedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Exclusive = true;
				rmq.ExchangeType = ExchangeType.Fanout;
			}
		}
	}
}
