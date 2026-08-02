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
	public class MappingOutboxNotificationPublisher<TDomainEvent, TIntegrationEvent> : IOutboxNotificationPublisher<TDomainEvent>
		where TDomainEvent : IIdempotentOutboxMessage
	{
		private readonly IOutboxSettingsProvider<TDomainEvent> _settingsProvider;
		private readonly IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent> _integrationEventBuilder;
		private readonly Dictionary<int, OutboxMessage> _sessionLastEvents = new Dictionary<int, OutboxMessage>();

		public MappingOutboxNotificationPublisher(
			IOutboxSettingsProvider<TDomainEvent> settingsProvider,
			IIntegrationEventBuilder<TDomainEvent, TIntegrationEvent> integrationEventBuilder
		)
		{
			_settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
			_integrationEventBuilder = integrationEventBuilder ?? throw new ArgumentNullException(nameof(integrationEventBuilder));
		}

		public async Task<bool> TryPublishAsync(
			IUnitOfWork unitOfWork,
			TDomainEvent domainEvent,
			CancellationToken cancellationToken = default)
		{
			if(_settingsProvider.IsDisabled(domainEvent))
			{
				return false;
			}

			if(!_settingsProvider.IsDuplicateAllowed(domainEvent))
			{
				var hasSameNotification = unitOfWork.Session.Query<OutboxMessage>().Any(m => m.DeduplicationKey == domainEvent.GetDeduplicationKey());

				if(hasSameNotification)
				{
					return false;
				}
			}

            var integrationEvent = await _integrationEventBuilder.BuildAsync(domainEvent, cancellationToken);

			if(integrationEvent == null)
			{
				return false;
			}

			OutboxMessage outboxMessage;

			if(_sessionLastEvents.TryGetValue(domainEvent.GetAggregateId(), out var existing))
            {
                existing.SavePayload(integrationEvent);

                outboxMessage = existing;
            }
            else
            {
                outboxMessage = new OutboxMessage(domainEvent, integrationEvent);
            }

            await unitOfWork.SaveAsync(outboxMessage, cancellationToken: cancellationToken);

			_sessionLastEvents[domainEvent.GetAggregateId()] = outboxMessage;

			return true;
		}


		public bool TryPublish(IUnitOfWork unitOfWork, TDomainEvent notificationEvent)
		{
			return TryPublishAsync(unitOfWork, notificationEvent)
				.GetAwaiter()
				.GetResult();
		}
	}
}
