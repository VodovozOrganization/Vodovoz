using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Definitions
{
	public class CustomerDocumentSendConsumerDefinition : ConsumerDefinition<CustomerDocumentSendConsumer>
	{
		public CustomerDocumentSendConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.customer-document-send.consumer.docflow");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<CustomerDocumentSendConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<CustomerDocumentSendEvent>();
			}
		}
	}
}
