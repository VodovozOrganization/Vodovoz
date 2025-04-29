using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Documents.Consumers.Definitions
{
	public class DocumentTaskCreatedConsumerDefinition : ConsumerDefinition<DocumentTaskCreatedConsumer>
	{
		public DocumentTaskCreatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.document-task-created.consumer.documents");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<DocumentTaskCreatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<DocumentTaskCreatedEvent>();
			}
		}
	}
}
