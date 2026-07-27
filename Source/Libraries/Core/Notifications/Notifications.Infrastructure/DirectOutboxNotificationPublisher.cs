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
	public class DirectOutboxNotificationPublisher<T> : IOutboxNotificationPublisher<T>
		where T : IIdempotentOutboxMessage
	{
		private readonly IOutboxSettingsProvider<T> _settingsProvider;
		private readonly Dictionary<int, OutboxMessage> _sessionLastEvents = new Dictionary<int, OutboxMessage>();

		public DirectOutboxNotificationPublisher(
			IOutboxSettingsProvider<T> settingsProvider
		)
		{
			_settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
		}

		public async Task<bool> TryPublishAsync(
			IUnitOfWork unitOfWork,
			T message,
			CancellationToken cancellationToken = default)
		{
			if(_settingsProvider.IsDisabled(message))
			{
				return false;
			}

			if(!_settingsProvider.IsDuplicateAllowed(message))
			{
				var hasSameNotification = unitOfWork.Session.Query<OutboxMessage>().Any(m => m.DeduplicationKey == message.GetDeduplicationKey());

				if(hasSameNotification)
				{
					return false;
				}
			}

			OutboxMessage outboxMessage;

			if(_sessionLastEvents.TryGetValue(message.GetAggregateId(), out var existing))
            {
                existing.SavePayload(message);

                outboxMessage = existing;
            }
            else
            {
                outboxMessage = new OutboxMessage(message);
            }

            await unitOfWork.SaveAsync(outboxMessage, cancellationToken: cancellationToken);

			_sessionLastEvents[message.GetAggregateId()] = outboxMessage;

			return true;
		}

		public bool TryPublish(IUnitOfWork unitOfWork, T notificationEvent)
		{
			return TryPublishAsync(unitOfWork, notificationEvent)
				.GetAwaiter()
				.GetResult();
		}
	}
}
