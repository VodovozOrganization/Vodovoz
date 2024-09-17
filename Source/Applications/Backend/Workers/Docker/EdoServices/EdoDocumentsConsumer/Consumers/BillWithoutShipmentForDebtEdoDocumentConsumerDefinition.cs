using MassTransit;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillWithoutShipmentForDebtEdoDocumentConsumerDefinition : ConsumerDefinition<BillWithoutShipmentForDebtEdoDocumentConsumer>
	{
		public BillWithoutShipmentForDebtEdoDocumentConsumerDefinition()
		{
			EndpointName = InfoForCreatingBillWithoutShipmentForDebtEdo.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<BillWithoutShipmentForDebtEdoDocumentConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
