using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Contracts;
using TransactionalOutbox.Domain;

namespace Notifications.Infrastructure
{
	public class OutBoxNotificationPublisher<TDomainEvent, TIntegrationEvent> : IOutboxNotificationPublisher<TDomainEvent>
		where TDomainEvent : IOutboxDomainEvent
	{
		private readonly IOutBoxSettingsProvider<TDomainEvent> _settingsProvider;
		private readonly IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent> _integrationEventBuilder;
		private readonly Dictionary<int, OutboxMessage> _sessionLastEvents = new Dictionary<int, OutboxMessage>();

		public OutBoxNotificationPublisher(
			IOutBoxSettingsProvider<TDomainEvent> settingsProvider,
			IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent> integrationEventBuilder
		)
		{
			_settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
			_integrationEventBuilder = integrationEventBuilder ?? throw new ArgumentNullException(nameof(integrationEventBuilder));
		}

		public async Task PublishAsync(
			IUnitOfWork unitOfWork,
			TDomainEvent domainEvent,
			CancellationToken cancellationToken = default)
		{
			if(_settingsProvider.IsDisabled(domainEvent))
			{
				return;
			}

			if(!_settingsProvider.IsDuplicateAllowed(domainEvent))
			{
				var hasSameNotification = unitOfWork.Session.Query<OutboxMessage>().Any(m => m.DeduplicationKey == domainEvent.GetDeduplicationKey());

				if(hasSameNotification)
				{
					return;
				}
			}

            OutboxMessage outboxMessage;

            var integrationEvent = await _integrationEventBuilder.BuildAsync(domainEvent, cancellationToken);

            if(_sessionLastEvents.TryGetValue(domainEvent.GetAggregateId(), out var existing))
            {
                existing.SavePayload(integrationEvent);

                outboxMessage = existing;

            }
            else
            {
                outboxMessage = new OutboxMessage(domainEvent, integrationEvent); // await _outboxMessageBuilder.BuildAsync(domainEvent, cancellationToken);
            }

            await unitOfWork.SaveAsync(outboxMessage, cancellationToken: cancellationToken);

			_sessionLastEvents[domainEvent.GetAggregateId()] = outboxMessage;
		}


		public void Publish(IUnitOfWork unitOfWork, TDomainEvent notificationEvent)
		{
			PublishAsync(unitOfWork, notificationEvent)
				.GetAwaiter()
				.GetResult();
		}
	}
}
