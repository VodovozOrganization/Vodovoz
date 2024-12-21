using MassTransit;

namespace TaxcomEdoConsumer.Consumers
{
	public class TaxcomDocflowSendEventConsumerDefinition : ConsumerDefinition<TaxcomDocflowSendEventConsumer>
	{
		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<TaxcomDocflowSendEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
