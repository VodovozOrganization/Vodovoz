using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	public class CustomerDocumentSentConsumerDefinition : ConsumerDefinition<CustomerDocumentSentConsumer>
	{
		public CustomerDocumentSentConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.customer-document-sent.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<CustomerDocumentSentConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<CustomerDocumentSentEvent>();
			}
		}
	}
}
