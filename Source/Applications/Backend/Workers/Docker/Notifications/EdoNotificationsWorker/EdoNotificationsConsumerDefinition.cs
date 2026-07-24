using MassTransit;
using System;

namespace EdoNotificationsWorker
{
	public class EdoNotificationsConsumerDefinition : ConsumerDefinition<EdoNotificationsConsumer>
	{
		public EdoNotificationsConsumerDefinition()
		{
			EndpointName = "edo-notifications";
			ConcurrentMessageLimit = 1;
		}

		protected override void ConfigureConsumer(
			IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<EdoNotificationsConsumer> consumerConfigurator)
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
