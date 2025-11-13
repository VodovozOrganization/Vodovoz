using MassTransit;
using TaxcomEdo.Contracts.Documents;

namespace EdoDocumentsConsumer.Consumers
{
	public class BillWithoutShipmentForPaymentEdoDocumentConsumerDefinition
		: ConsumerDefinition<BillWithoutShipmentForPaymentEdoDocumentConsumer>
	{
		public BillWithoutShipmentForPaymentEdoDocumentConsumerDefinition()
		{
			EndpointName = InfoForCreatingBillWithoutShipmentForPaymentEdo.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<BillWithoutShipmentForPaymentEdoDocumentConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
