using MassTransit;
using NHibernate.Linq;
using Notifications.Core;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.MassTransit;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Публикатор уведомлений: строит сообщение из доменного события (<see cref="IIntegrationEventBuilder{TDomainEvent, TMessage}"/>)
	/// и пишет его в outbox шины уведомлений в транзакции UoW.
	/// Учитывает настройки отключения и дедупликации (ключи в notification_deduplication_keys), а в рамках одной транзакции
	/// оставляет только последнее уведомление по агрегату (<see cref="IDeduplicatedNotification.GetAggregateKey"/>).
	/// </summary>
	/// <typeparam name="TDomainEvent">Доменное событие</typeparam>
	/// <typeparam name="TMessage">Сообщение в шину; если публикуется само событие — тот же тип и <see cref="SameEventBuilder{TEvent}"/></typeparam>
	public class NotificationPublisher<TDomainEvent, TMessage> : INotificationPublisher<TDomainEvent>
		where TDomainEvent : IDeduplicatedNotification
		where TMessage : class
	{
		private readonly INotificationSettingsProvider<TDomainEvent> _settingsProvider;
		private readonly IIntegrationEventBuilder<TDomainEvent, TMessage> _messageBuilder;
		// IUnitOfWorkOutbox: уведомления идут во вторую шину приложения (INotificationsBus, другой vhost RabbitMQ — notifications).
		// IPublishEndpoint из DI связан только с основной шиной,
		// поэтому сообщение пишется в таблицы outbox этой шины через IUnitOfWorkOutbox в транзакции UoW. При переходе на один vhost заменить на IPublishEndpoint из DI
		private readonly IUnitOfWorkOutbox<INotificationsBus> _outbox;
		private readonly ConditionalWeakTable<IUnitOfWork, Dictionary<string, Guid>> _lastMessageIdsByAggregate =
			new ConditionalWeakTable<IUnitOfWork, Dictionary<string, Guid>>();

		public NotificationPublisher(
			INotificationSettingsProvider<TDomainEvent> settingsProvider,
			IIntegrationEventBuilder<TDomainEvent, TMessage> messageBuilder,
			IUnitOfWorkOutbox<INotificationsBus> outbox)
		{
			_settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
			_messageBuilder = messageBuilder ?? throw new ArgumentNullException(nameof(messageBuilder));
			_outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
		}

		public async Task<bool> TryPublishAsync(
			IUnitOfWork unitOfWork,
			TDomainEvent domainEvent,
			CancellationToken cancellationToken = default)
		{
			var settings = _settingsProvider.GetSendingSettings(domainEvent);

			if(settings.IsDisabled)
			{
				return false;
			}

			var deduplicationKey = domainEvent.GetDeduplicationKey()?.Trim();
			var isKeyRegistered = !string.IsNullOrEmpty(deduplicationKey)
				&& await IsKeyRegisteredAsync(unitOfWork, deduplicationKey, cancellationToken);

			if(isKeyRegistered && !settings.IsDuplicateAllowed)
			{
				return false;
			}

			var message = await _messageBuilder.BuildAsync(domainEvent, cancellationToken);

			if(message == null)
			{
				return false;
			}

			var lastMessageIds = _lastMessageIdsByAggregate.GetOrCreateValue(unitOfWork);

			if(lastMessageIds.TryGetValue(domainEvent.GetAggregateKey(), out var previousMessageId))
			{
				_outbox.TryRemove(unitOfWork, previousMessageId);
			}

			var messageId = NewId.NextGuid();

			await _outbox.GetPublishEndpoint(unitOfWork)
				.Publish(message, context => context.MessageId = messageId, cancellationToken);

			lastMessageIds[domainEvent.GetAggregateKey()] = messageId;

			if(!string.IsNullOrEmpty(deduplicationKey) && !isKeyRegistered)
			{
				await unitOfWork.SaveAsync(new NotificationDeduplicationKey(deduplicationKey), false, cancellationToken);
			}

			return true;
		}

		public bool TryPublish(IUnitOfWork unitOfWork, TDomainEvent domainEvent)
		{
			return TryPublishAsync(unitOfWork, domainEvent)
				.GetAwaiter()
				.GetResult();
		}

		private static Task<bool> IsKeyRegisteredAsync(IUnitOfWork unitOfWork, string deduplicationKey, CancellationToken cancellationToken) =>
			unitOfWork.Session.Query<NotificationDeduplicationKey>()
				.AnyAsync(x => x.Key == deduplicationKey, cancellationToken);
	}
}
