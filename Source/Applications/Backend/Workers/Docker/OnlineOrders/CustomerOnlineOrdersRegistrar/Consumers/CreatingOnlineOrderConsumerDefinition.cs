using CustomerOrdersApi.Library.V4.Dto.Orders;
using MassTransit;

namespace CustomerOnlineOrdersRegistrar.Consumers
{
	public class CreatingOnlineOrderConsumerDefinition : ConsumerDefinition<CreatingOnlineOrderConsumer>
	{
		public CreatingOnlineOrderConsumerDefinition()
		{
			EndpointName = CreatingOnlineOrder.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<CreatingOnlineOrderConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
			endpointConfigurator.ConcurrentMessageLimit = 1;
			endpointConfigurator.PrefetchCount = 1;
		}
	}
}
