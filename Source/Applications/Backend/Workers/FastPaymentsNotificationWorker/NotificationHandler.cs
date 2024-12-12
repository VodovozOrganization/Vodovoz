using FastPaymentsAPI.Library.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.FastPayments;
using Vodovoz.EntityRepositories.FastPayments;

namespace FastPaymentsNotificationWorker
{
	public class NotificationHandler
	{
		private readonly ILogger<NotificationHandler> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly SiteNotifier _siteNotifier;
		private readonly MobileAppNotifier _mobileAppNotifier;
		private readonly IEnumerable<int> _repeatsTimeline;

		public NotificationHandler(
			ILogger<NotificationHandler> logger,
			IServiceScopeFactory serviceScopeFactory,
			IConfiguration configuration,
			IUnitOfWorkFactory uowFactory,
			SiteNotifier siteNotifier,
			MobileAppNotifier mobileAppNotifier)
		{
			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_siteNotifier = siteNotifier ?? throw new ArgumentNullException(nameof(siteNotifier));
			_mobileAppNotifier = mobileAppNotifier ?? throw new ArgumentNullException(nameof(mobileAppNotifier));

			_repeatsTimeline = configuration.GetSection("NotifyWorker")
				.GetSection("NotifierRepeatOnTimelineInMinutes")
				.GetChildren()
				.Select(x => x.Get<int>());
		}

		public async Task HandleNotifications(CancellationToken cancellationToken)
		{
			using var messageHandlingScope = _serviceScopeFactory.CreateScope();
			using var uow = _uowFactory.CreateWithoutRoot();
			var fastPaymentRepository = messageHandlingScope.ServiceProvider.GetRequiredService<IFastPaymentRepository>();
			var notifications = fastPaymentRepository.GetActiveNotifications(uow);

			foreach (var notification in notifications)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					return;
				}

				await TryRepeatNotification(notification);
			}
		}

		private async Task TryRepeatNotification(FastPaymentNotification notification)
		{
			var canRepeatNotification = CanRepeatNotificationNow(notification);
			if(!canRepeatNotification)
			{
				return;
			}

			switch(notification.Type)
			{
				case FastPaymentNotificationType.Site:
					await _siteNotifier.RepeatNotifyPaymentStatusChangeAsync(notification);
					break;
				case FastPaymentNotificationType.MobileApp:
					await _mobileAppNotifier.RepeatNotifyPaymentStatusChangeAsync(notification);
					break;
				default:
					throw new NotSupportedException("Не поддерживаемый тип уведомления быстрой оплаты");
			}
		}

		private bool CanRepeatNotificationNow(FastPaymentNotification notification)
		{
			if(notification.LastTryTime == null)
			{
				return false;
			}

			var timeFromLastTry = notification.LastTryTime.Value - notification.Time;
			var timeFromCreation = DateTime.Now - notification.Time;

			if(timeFromCreation.TotalSeconds < 0)
			{
				_logger.LogWarning("Невозможно обработать уведомление. Дата создания уведомления больше текущей даты.");
			}

			if(timeFromLastTry.TotalSeconds < 0)
			{
				_logger.LogWarning("Невозможно обработать уведомление. Дата последней попытки уведомления больше текущей даты.");
			}

			var needStopNotifications = _repeatsTimeline.Last() <= timeFromLastTry.TotalMinutes;

			foreach(var plannedRepeatTime in _repeatsTimeline)
			{
				var alreadyHasAttempt = plannedRepeatTime < timeFromLastTry.TotalMinutes;

				var canTryRepeat = plannedRepeatTime < timeFromCreation.TotalMinutes;

				var canRepeatNotification = !alreadyHasAttempt && canTryRepeat;
				if(canRepeatNotification)
				{
					return true;
				}
			}

			if(needStopNotifications)
			{
				StopNotifications(notification.Id);
			}
			
			return false;
		}

		private void StopNotifications(int notificationId)
		{
			using var uow = _uowFactory.CreateWithoutRoot();
			var notification = uow.GetById<FastPaymentNotification>(notificationId);
			notification.StopNotifications = true;
			uow.Save(notification);
			uow.Commit();
		}
	}
}
