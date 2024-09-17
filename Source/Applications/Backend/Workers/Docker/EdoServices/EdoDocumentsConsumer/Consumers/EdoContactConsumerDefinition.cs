using MassTransit;
using TaxcomEdo.Contracts.Counterparties;

namespace EdoDocumentsConsumer.Consumers
{
	public class EdoContactConsumerDefinition : ConsumerDefinition<EdoContactConsumer>
	{
		public EdoContactConsumerDefinition()
		{
			EndpointName = EdoContactInfo.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EdoContactConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
