using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Contracts;
using TransactionalOutbox.Domain;

namespace PushNotifications.Infrastructure
{
	public class OutBoxPushNotificationPublisher<TDomainEvent> : IOutboxPushNotificationPublisher<TDomainEvent>
		where TDomainEvent : IOutboxDomainEvent
	{
		private readonly IOutBoxSettingsProvider<TDomainEvent> _settingsProvider;
		private readonly IOutboxMessageBuilder<TDomainEvent> _outboxMessageBuilder;
		private readonly Dictionary<int, OutboxMessage> _sessionLastEvents = new Dictionary<int, OutboxMessage>();

		public OutBoxPushNotificationPublisher(
			IOutBoxSettingsProvider<TDomainEvent> settingsProvider,
			IOutboxMessageBuilder<TDomainEvent> outboxMessageBuilder
		)
		{
			_settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
			_outboxMessageBuilder = outboxMessageBuilder ?? throw new ArgumentNullException(nameof(outboxMessageBuilder));
		}

		public async Task PublishAsync(
			IUnitOfWork unitOfWork,
			TDomainEvent notificationEvent,
			CancellationToken cancellationToken = default)
		{
			if(_settingsProvider.IsDisabled(notificationEvent))
			{
				return;
			}

			if(_sessionLastEvents.TryGetValue(notificationEvent.GetAggregateId(), out var existing))
			{
				//await unitOfWork.DeleteAsync(existing, cancellationToken); 
				await unitOfWork.Session.FlushAsync(cancellationToken);
				await unitOfWork.Session.EvictAsync(existing, cancellationToken);
				await unitOfWork.Session.FlushAsync(cancellationToken);
			}

			if(!_settingsProvider.IsDuplicateAllowed(notificationEvent))
			{
				var hasSameNotification = unitOfWork.Session.Query<OutboxMessage>().Any(m => m.DeduplicationKey == notificationEvent.GetDeduplicationKey());

				if(hasSameNotification)
				{
					// return;
				}
			}
			
			var outboxMessage = await _outboxMessageBuilder.BuildAsync(notificationEvent, cancellationToken);
			
			await unitOfWork.SaveAsync(outboxMessage, false, cancellationToken);

			_sessionLastEvents[notificationEvent.GetAggregateId()] = outboxMessage;
		}


		public void Publish(IUnitOfWork unitOfWork, TDomainEvent notificationEvent)
		{
			PublishAsync(unitOfWork, notificationEvent)
				.GetAwaiter()
				.GetResult();
		}
	}
}
