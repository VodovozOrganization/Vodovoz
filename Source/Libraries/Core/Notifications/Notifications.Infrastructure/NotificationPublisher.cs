using MassTransit;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.Abstractions;

namespace Notifications.Infrastructure
{
	public class NotificationPublisher<TDomainEvent, TBus, TIntegrationEvent> : INotificationPublisher<TDomainEvent>
		where TBus : class, IBus
	{
		private readonly TBus _bus;
		private readonly IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent> _customerNotificationsIntegrationEventBuilder;

		public NotificationPublisher(
			TBus bus,
			IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent> customerNotificationsIntegrationEventBuilder)
		{
			_bus = bus ?? throw new System.ArgumentNullException(nameof(bus));
			_customerNotificationsIntegrationEventBuilder = customerNotificationsIntegrationEventBuilder 
				?? throw new System.ArgumentNullException(nameof(customerNotificationsIntegrationEventBuilder));
		}

		public async Task PublishAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
		{
			var integrationEvent = await _customerNotificationsIntegrationEventBuilder.BuildAsync(domainEvent, cancellationToken);

			if(integrationEvent == null)
			{
				return;
			}

			await _bus.Publish(integrationEvent, cancellationToken);
		}
	}
}
