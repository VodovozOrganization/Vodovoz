using CustomerOrdersApi.Library.V5.Dto.Orders;
using MassTransit;

namespace CustomerOnlineOrdersRegistrar.Consumers
{
	public class CreatingOnlineOrderWithTemplateConsumerDefinition : ConsumerDefinition<CreatingOnlineOrderWithTemplateConsumer>
	{
		public CreatingOnlineOrderWithTemplateConsumerDefinition()
		{
			EndpointName = CreatingOnlineOrder.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<CreatingOnlineOrderWithTemplateConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
			endpointConfigurator.ConcurrentMessageLimit = 1;
			endpointConfigurator.PrefetchCount = 1;
		}
	}
}
