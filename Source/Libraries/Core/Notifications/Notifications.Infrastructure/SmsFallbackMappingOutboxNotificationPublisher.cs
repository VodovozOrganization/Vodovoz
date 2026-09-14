using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.Abstractions;
using TransactionalOutbox.Contracts;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Публикация пуш уведомлений по событию в рамках транзакционного аутбокса 
	/// с возможностью создания смс уведомлений в случаях, когда пользователь не использует мобильное приложение
	/// </summary>
	/// <typeparam name="TDomainEvent">Тип доменного события</typeparam>
	public class MappingOutboxNotificationWithSmsFallbackPublisher<TDomainEvent> : IOutboxNotificationPublisher<TDomainEvent>
		where TDomainEvent : IIdempotentOutboxMessage
	{
		private readonly ILogger<MappingOutboxNotificationWithSmsFallbackPublisher<TDomainEvent>> _logger;
		private readonly IOutboxNotificationPublisher<TDomainEvent> _innerPublisher;
		private readonly IOutboxSettingsProvider<TDomainEvent> _settingsProvider;
		private readonly ISmsNotificationSendingPolicy _smsNotificationSendingPolicy;
		private readonly IEnumerable<ISmsNotificationCreator<TDomainEvent>> _smsNotificationCreators;

		public MappingOutboxNotificationWithSmsFallbackPublisher(
			ILogger<MappingOutboxNotificationWithSmsFallbackPublisher<TDomainEvent>> logger,
			IOutboxNotificationPublisher<TDomainEvent> innerPublisher,
			IOutboxSettingsProvider<TDomainEvent> settingsProvider,
			ISmsNotificationSendingPolicy smsNotificationSendingPolicy,
			IEnumerable<ISmsNotificationCreator<TDomainEvent>> smsNotificationCreators)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_innerPublisher = innerPublisher ?? throw new ArgumentNullException(nameof(innerPublisher));
			_settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
			_smsNotificationSendingPolicy =
				smsNotificationSendingPolicy ?? throw new ArgumentNullException(nameof(smsNotificationSendingPolicy));
			_smsNotificationCreators =
				smsNotificationCreators ?? throw new ArgumentNullException(nameof(smsNotificationCreators));
		}

		/// <inheritdoc />
		public async Task<bool> TryPublishAsync(
			IUnitOfWork unitOfWork,
			TDomainEvent notificationEvent,
			CancellationToken cancellationToken = default)
		{
			var published = await _innerPublisher.TryPublishAsync(unitOfWork, notificationEvent, cancellationToken);

			await TryCreateSmsNotificationsAsync(unitOfWork, notificationEvent, cancellationToken);

			return published;
		}

		/// <inheritdoc />
		public bool TryPublish(IUnitOfWork unitOfWork, TDomainEvent notificationEvent)
		{
			return TryPublishAsync(unitOfWork, notificationEvent)
				.GetAwaiter()
				.GetResult();
		}

		/// <summary>
		/// Создаёт смс уведомления, если для события это предусмотрено и соблюдены условия для создания смс уведомлений
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="notificationEvent"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task TryCreateSmsNotificationsAsync(
			IUnitOfWork unitOfWork,
			TDomainEvent notificationEvent,
			CancellationToken cancellationToken)
		{
			try
			{
				if(notificationEvent == null)
				{
					return;
				}

				var applicableCreators = _smsNotificationCreators
					.Where(creator => creator.CanCreate(notificationEvent))
					.ToArray();

				if(!applicableCreators.Any())
				{
					return;
				}

				if(_settingsProvider.IsDisabled(notificationEvent))
				{
					_logger.LogInformation(
						"Уведомления по событию {DeduplicationKey} отключены настройкой, смс уведомление не создаётся",
						notificationEvent.GetDeduplicationKey());

					return;
				}

				if(!_smsNotificationSendingPolicy.IsSmsSendingEnabled)
				{
					_logger.LogInformation(
						"Отправка смс уведомлений отключена настройкой, "
						+ "смс уведомление по событию {DeduplicationKey} не создаётся",
						notificationEvent.GetDeduplicationKey());

					return;
				}

				foreach(var creator in applicableCreators)
				{
					await creator.CreateAsync(unitOfWork, notificationEvent, cancellationToken);
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при создании смс уведомления по событию типа {DomainEventType}",
					typeof(TDomainEvent).Name);
			}
		}
	}
}
