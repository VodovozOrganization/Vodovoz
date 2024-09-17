using MassTransit;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillEdoDocumentConsumerDefinition : ConsumerDefinition<BillEdoDocumentConsumer>
	{
		public BillEdoDocumentConsumerDefinition()
		{
			EndpointName = InfoForCreatingEdoBill.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<BillEdoDocumentConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
