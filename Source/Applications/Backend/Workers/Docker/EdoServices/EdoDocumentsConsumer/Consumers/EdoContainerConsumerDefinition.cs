using MassTransit;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class EdoContainerConsumerDefinition : ConsumerDefinition<EdoContainerConsumer>
	{
		public EdoContainerConsumerDefinition()
		{
			EndpointName = EdoContainerInfo.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EdoContainerConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
