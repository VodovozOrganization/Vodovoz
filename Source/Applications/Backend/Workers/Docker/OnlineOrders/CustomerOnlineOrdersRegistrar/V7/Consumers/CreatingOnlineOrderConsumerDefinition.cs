using CustomerOrdersApi.Library.V7.Dto.Orders;
using MassTransit;

namespace CustomerOnlineOrdersRegistrar.V7.Consumers
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
