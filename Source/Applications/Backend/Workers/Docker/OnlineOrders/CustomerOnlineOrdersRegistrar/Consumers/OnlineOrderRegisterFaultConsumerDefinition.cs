using MassTransit;

namespace CustomerOnlineOrdersRegistrar.Consumers
{
	public class OnlineOrderRegisterFaultConsumerDefinition : ConsumerDefinition<OnlineOrderRegisterFaultConsumer>
	{
		public OnlineOrderRegisterFaultConsumerDefinition()
		{
			EndpointName = "online-orders-fault";
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OnlineOrderRegisterFaultConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
			endpointConfigurator.PrefetchCount = 1;
			endpointConfigurator.UseMessageRetry(r => r.Interval(10, 5000));
		}
	}
}
