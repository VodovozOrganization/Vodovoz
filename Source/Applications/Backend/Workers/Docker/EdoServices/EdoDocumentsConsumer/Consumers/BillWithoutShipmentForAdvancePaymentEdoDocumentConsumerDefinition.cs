using MassTransit;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillWithoutShipmentForAdvancePaymentEdoDocumentConsumerDefinition
		: ConsumerDefinition<BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer>
	{
		public BillWithoutShipmentForAdvancePaymentEdoDocumentConsumerDefinition()
		{
			EndpointName = InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
