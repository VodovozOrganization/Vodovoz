using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	public class CustomerDocumentSentConsumerDefinition : ConsumerDefinition<CustomerDocumentSentConsumer>
	{
		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<CustomerDocumentSentConsumer> consumerConfigurator)
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
