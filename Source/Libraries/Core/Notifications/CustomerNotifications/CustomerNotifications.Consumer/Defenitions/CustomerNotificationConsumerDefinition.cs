using System;
using CustomerNotifications.Consumer.Consumers;
using CustomerNotifications.Contracts;
using MassTransit;

namespace CustomerNotifications.Consumer.Defenitions
{
	public class CustomerNotificationConsumerDefinition
		: ConsumerDefinition<CustomerNotificationConsumer>
	{
		public CustomerNotificationConsumerDefinition()
		{
			EndpointName = CustomerNotificationsConstants.QueueName;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<CustomerNotificationConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = true;
			endpointConfigurator.PrefetchCount = 8;
			endpointConfigurator.ConcurrentMessageLimit = 4;
			endpointConfigurator.UseInMemoryOutbox();

			endpointConfigurator.UseDelayedRedelivery(r =>
			{
				r.Intervals(
					TimeSpan.FromSeconds(5),
					TimeSpan.FromSeconds(15),
					TimeSpan.FromSeconds(45),
					TimeSpan.FromMinutes(2),
					TimeSpan.FromMinutes(10));
			});

			endpointConfigurator.UseMessageRetry(r => r.Immediate(0));
		}
	}
}
