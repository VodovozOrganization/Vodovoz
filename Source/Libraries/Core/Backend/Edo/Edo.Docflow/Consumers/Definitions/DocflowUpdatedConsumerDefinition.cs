using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Definitions
{
	public class DocflowUpdatedConsumerDefinition : ConsumerDefinition<DocflowUpdatedConsumer>
	{
		public DocflowUpdatedConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.docflow-updated.consumer.docflow");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<DocflowUpdatedConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<EdoDocflowUpdatedEvent>();
			}
		}
	}
}
