using Edo.Contracts.Messages.Events;
using MassTransit;
using RabbitMQ.Client;

namespace Edo.Docflow.Consumers.Definitions
{
	public class DocflowUpdatedErrorConsumerDefinition : ConsumerDefinition<DocflowUpdatedErrorConsumer>
	{
		public DocflowUpdatedErrorConsumerDefinition()
		{
			Endpoint(x => x.Name = "edo.docflow-updated.consumer.docflow_error");
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<DocflowUpdatedErrorConsumer> consumerConfigurator)
		{
			var rmq = (IRabbitMqReceiveEndpointConfigurator)endpointConfigurator;

			endpointConfigurator.ConfigureConsumeTopology = false;
			rmq.PrefetchCount = 1;
		}
	}
}
