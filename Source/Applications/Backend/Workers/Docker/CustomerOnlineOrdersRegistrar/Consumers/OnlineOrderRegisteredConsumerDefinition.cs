using MassTransit;

namespace CustomerOnlineOrdersRegistrar.Consumers
{
	public class OnlineOrderRegisteredConsumerDefinition : ConsumerDefinition<OnlineOrderRegisteredConsumer>
	{
		public OnlineOrderRegisteredConsumerDefinition()
		{
			EndpointName = "online-orders";
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OnlineOrderRegisteredConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
			endpointConfigurator.ConcurrentMessageLimit = 1;
			endpointConfigurator.PrefetchCount = 1;
		}
	}
}
