using MassTransit;

namespace CustomerOnlineOrdersRegistrar.V3.Consumers
{
	/// <summary>
	/// Описание настроек обработчика онлайн заказов, упавших при первой обработке
	/// </summary>
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
