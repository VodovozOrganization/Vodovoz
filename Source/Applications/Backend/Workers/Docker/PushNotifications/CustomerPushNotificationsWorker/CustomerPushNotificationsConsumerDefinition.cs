using MassTransit;
using System;

namespace CustomerNotificationsWorker
{
	public class CustomerPushNotificationsConsumerDefinition : ConsumerDefinition<CustomerPushNotificationsConsumer>
	{
		public CustomerPushNotificationsConsumerDefinition()
		{
			EndpointName = "customer-push-notifications";
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<CustomerPushNotificationsConsumer> consumerConfigurator)
		{
			endpointConfigurator.UseMessageRetry(r =>
			{
				r.Handle<Exception>();

				r.Intervals(
					TimeSpan.FromSeconds(5),
					TimeSpan.FromSeconds(15),
					TimeSpan.FromSeconds(45),
					TimeSpan.FromMinutes(2),
					TimeSpan.FromMinutes(10)
				);
			});
		}
	}
}
