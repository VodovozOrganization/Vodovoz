using MassTransit;

namespace CustomerOnlineOrdersRegistrar.V3.Consumers
{
	/// <summary>
	/// Описание настроек обработчика онлайн заказов
	/// </summary>
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
