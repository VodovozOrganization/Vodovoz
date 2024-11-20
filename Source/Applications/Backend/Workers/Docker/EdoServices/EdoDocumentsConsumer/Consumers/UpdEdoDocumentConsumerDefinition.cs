using MassTransit;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class UpdEdoDocumentConsumerDefinition : ConsumerDefinition<UpdEdoDocumentConsumer>
	{
		public UpdEdoDocumentConsumerDefinition()
		{
			EndpointName = InfoForCreatingEdoUpd.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<UpdEdoDocumentConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
