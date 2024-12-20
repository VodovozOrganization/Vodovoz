using Edo.Transport2;
using MassTransit;

namespace TaxcomEdoConsumer.Consumers
{
	public class EdoDocflowUpdatedEventConsumerDefinition : ConsumerDefinition<EdoDocflowUpdatedEventConsumer>
	{
		public EdoDocflowUpdatedEventConsumerDefinition()
		{
			EndpointName = nameof(EdoDocflowUpdatedEvent);
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EdoDocflowUpdatedEventConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
		}
	}
}
