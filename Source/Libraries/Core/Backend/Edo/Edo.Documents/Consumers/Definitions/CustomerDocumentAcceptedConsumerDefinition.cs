using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	public class CustomerDocumentAcceptedConsumerDefinition : ConsumerDefinition<CustomerDocumentAcceptedConsumer>
	{
		public CustomerDocumentAcceptedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.customer-document-accepted.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<CustomerDocumentAcceptedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<CustomerDocumentAcceptedEvent>();
			}
		}
	}
}
