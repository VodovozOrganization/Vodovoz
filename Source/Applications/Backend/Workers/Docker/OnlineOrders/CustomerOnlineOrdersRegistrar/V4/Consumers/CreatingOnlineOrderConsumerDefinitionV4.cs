using CustomerOrdersApi.Library.V4.Dto.Orders;
using MassTransit;

namespace CustomerOnlineOrdersRegistrar.V4.Consumers
{
	public class CreatingOnlineOrderConsumerDefinitionV4 : ConsumerDefinition<CreatingOnlineOrderConsumerV4>
	{
		public CreatingOnlineOrderConsumerDefinitionV4()
		{
			EndpointName = CreatingOnlineOrder.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<CreatingOnlineOrderConsumerV4> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
			endpointConfigurator.ConcurrentMessageLimit = 1;
			endpointConfigurator.PrefetchCount = 1;
		}
	}
}
