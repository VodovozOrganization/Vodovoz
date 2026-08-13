using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;

namespace Edo.Receipt.Sender
{
	/// <summary>
	/// Сервис уведомлений об ошибке отправки чеков в кассу.
	/// </summary>
	public class ReceiptSendingFailedNotificationService
	{
		private readonly ILogger<ReceiptSendingFailedNotificationService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly IEdoNotificationMessageFactory _notificationMessageFactory;

		public ReceiptSendingFailedNotificationService(
			ILogger<ReceiptSendingFailedNotificationService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher,
			IEdoNotificationMessageFactory notificationMessageFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_notificationPublisher = notificationPublisher
				?? throw new ArgumentNullException(nameof(notificationPublisher));
			_notificationMessageFactory = notificationMessageFactory
				?? throw new ArgumentNullException(nameof(notificationMessageFactory));
		}

		public async Task NotifyAsync(
			int edoTaskId,
			int? orderId,
			int? cashboxId,
			IReadOnlyCollection<string> problemSourceNames,
			string details,
			CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(ReceiptSendingFailedNotificationService)))
			{
				var notification = _notificationMessageFactory.Create(
					EdoNotificationType.ReceiptSendingFailed,
					("EdoTaskId", edoTaskId.ToString(CultureInfo.InvariantCulture)),
					("OrderId", orderId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
					("CashboxId", cashboxId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty),
					("ProblemSources", problemSourceNames == null || problemSourceNames.Count == 0
						? string.Empty
						: string.Join(", ", problemSourceNames)),
					("Details", details ?? string.Empty),
					("ProblemMessage", "Не удалось отправить один или несколько чеков в кассу"));

				var published = await _notificationPublisher.TryPublishAsync(
					uow,
					notification,
					cancellationToken);

				if(!published)
				{
					_logger.LogWarning(
						"Уведомление об ошибке отправки чека по задаче №{EdoTaskId} не опубликовано " +
						"(уведомление отключено, нет настройки или уже отправлялось)",
						edoTaskId);
					return;
				}

				await uow.CommitAsync(cancellationToken);

				_logger.LogInformation(
					"Опубликовано уведомление об ошибке отправки чека по задаче №{EdoTaskId}",
					edoTaskId);
			}
		}
	}
}
