using Edo.Transport2;
using MassTransit;

namespace TaxcomEdoConsumer.Consumers
{
	public class OutgoingTaxcomDocflowUpdatedEventConsumerDefinition : ConsumerDefinition<OutgoingTaxcomDocflowUpdatedEventConsumer>
	{
		public OutgoingTaxcomDocflowUpdatedEventConsumerDefinition()
		{
			EndpointName = nameof(OutgoingTaxcomDocflowUpdatedEvent);
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OutgoingTaxcomDocflowUpdatedEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
