using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	public class OrderDocumentAcceptedConsumerDefinition : ConsumerDefinition<OrderDocumentAcceptedConsumer>
	{
		public OrderDocumentAcceptedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.customer-document-accepted.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OrderDocumentAcceptedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OrderDocumentAcceptedEvent>();
			}
		}
	}
}
