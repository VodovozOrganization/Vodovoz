using Edo.Transport2;
using MassTransit;

namespace TaxcomEdoConsumer.Consumers
{
	public class TaxcomDocflowSendEventConsumerDefinition : ConsumerDefinition<TaxcomDocflowSendEventConsumer>
	{
		public TaxcomDocflowSendEventConsumerDefinition()
		{
			EndpointName = nameof(TaxcomDocflowSendEvent);
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TaxcomDocflowSendEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
