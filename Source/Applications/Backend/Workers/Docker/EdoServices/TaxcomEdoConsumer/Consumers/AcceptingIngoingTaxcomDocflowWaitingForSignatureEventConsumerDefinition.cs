using Edo.Transport2;
using MassTransit;

namespace TaxcomEdoConsumer.Consumers
{
	public class AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumerDefinition
		: ConsumerDefinition<AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer>
	{
		public AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumerDefinition()
		{
			EndpointName = nameof(AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent);
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<AcceptingIngoingTaxcomDocflowWaitingForSignatureEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
