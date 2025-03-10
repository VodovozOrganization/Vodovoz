using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	public class OrderDocumentSentConsumerDefinition : ConsumerDefinition<OrderDocumentSentConsumer>
	{
		public OrderDocumentSentConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.order-document-sent.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OrderDocumentSentConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OrderDocumentSentEvent>();
			}
		}
	}
}
