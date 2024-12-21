using Edo.Transport2;
using MassTransit;

namespace TaxcomEdoConsumer.Consumers
{
	public class AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumerDefinition
		: ConsumerDefinition<AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer>
	{
		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
