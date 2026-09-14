using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Fault
{
	public class FaultDocumentTaskCreatedConsumerDefinition : ConsumerDefinition<FaultDocumentTaskCreatedConsumer>
	{
		public FaultDocumentTaskCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.document-task-created.consumer.documents_error");
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<FaultDocumentTaskCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;
				rmq.Bind<Fault<DocumentTaskCreatedEvent>>();
			}
		}
	}
}
