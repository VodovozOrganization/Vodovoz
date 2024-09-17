using MassTransit;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillWithoutShipmentForAdvancePaymentEdoDocumentConsumerDefinition
		: ConsumerDefinition<BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer>
	{
		public BillWithoutShipmentForAdvancePaymentEdoDocumentConsumerDefinition()
		{
			EndpointName = InfoForCreatingBillWithoutShipmentForPaymentEdo.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<BillWithoutShipmentForAdvancePaymentEdoDocumentConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
