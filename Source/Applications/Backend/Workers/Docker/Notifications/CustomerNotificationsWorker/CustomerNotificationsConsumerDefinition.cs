using MassTransit;
using System;

namespace CustomerNotificationsWorker
{
	public class CustomerNotificationsConsumerDefinition :
		ConsumerDefinition<CustomerNotificationsConsumer>
	{
		public CustomerNotificationsConsumerDefinition()
		{
			EndpointName = "customer-notifications";
			ConcurrentMessageLimit = 5;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<CustomerNotificationsConsumer> consumerConfigurator)
		{
			endpointConfigurator.UseMessageRetry(r =>
			{
				r.Intervals(
					TimeSpan.FromSeconds(10),
					TimeSpan.FromMinutes(1),
					TimeSpan.FromMinutes(3),
					TimeSpan.FromMinutes(10)
				);

				r.Handle<Exception>();

				r.Ignore<ArgumentOutOfRangeException>();
				r.Ignore<NotImplementedException>();
				r.Ignore<NotSupportedException>();
				r.Ignore<ArgumentNullException>();
			});
		}
	}
}
