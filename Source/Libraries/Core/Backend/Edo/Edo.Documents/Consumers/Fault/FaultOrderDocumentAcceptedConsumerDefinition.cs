using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Fault
{
	public class FaultOrderDocumentAcceptedConsumerDefinition : ConsumerDefinition<FaultOrderDocumentAcceptedConsumer>
	{
		public FaultOrderDocumentAcceptedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.customer-document-accepted.consumer.documents_fault");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<FaultOrderDocumentAcceptedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Bind<Fault<OrderDocumentAcceptedEvent>>();
			}
		}
	}
}
