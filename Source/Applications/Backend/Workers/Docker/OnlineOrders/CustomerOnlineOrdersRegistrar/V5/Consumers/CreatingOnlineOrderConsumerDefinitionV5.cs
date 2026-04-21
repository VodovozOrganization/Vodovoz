using CustomerOrdersApi.Library.V5.Dto.Orders;
using MassTransit;

namespace CustomerOnlineOrdersRegistrar.V5.Consumers
{
	/// <summary>
	/// Описание настроек обработчика онлайн заказов
	/// </summary>
	public class CreatingOnlineOrderConsumerDefinitionV5 : ConsumerDefinition<CreatingOnlineOrderConsumerV5>
	{
		public CreatingOnlineOrderConsumerDefinitionV5()
		{
			EndpointName = CreatingOnlineOrder.ExchangeAndQueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<CreatingOnlineOrderConsumerV5> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;
			endpointConfigurator.ConcurrentMessageLimit = 1;
			endpointConfigurator.PrefetchCount = 1;
		}
	}
}
