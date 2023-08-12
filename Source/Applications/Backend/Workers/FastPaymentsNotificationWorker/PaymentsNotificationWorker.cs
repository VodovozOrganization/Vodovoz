using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace FastPaymentsNotificationWorker
{
	public class PaymentsNotificationWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<PaymentsNotificationWorker> _logger;
		private readonly NotificationHandler _notificationHandler;
		private readonly TimeSpan _interval;
		private bool _isRunning = false;

		public PaymentsNotificationWorker(ILogger<PaymentsNotificationWorker> logger, NotificationHandler notificationHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_notificationHandler = notificationHandler ?? throw new ArgumentNullException(nameof(notificationHandler));
			_interval = TimeSpan.FromSeconds(60);
		}
		protected override TimeSpan Interval => _interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(_isRunning)
			{
				return;
			}

			_isRunning = true;

			try
			{
				_logger.LogInformation("Вызов обработки уведомлений смены статуса быстрой оплаты");
				await _notificationHandler.HandleNotifications(stoppingToken);
			}
			catch(Exception ex)
			{
				_logger.LogCritical(ex, "Поймано необработанное исключение");
			}
			finally
			{
				_isRunning = false;
			}
		}

		protected override void OnStartService()
		{
			_logger.LogInformation("Запущен сервис обработки уведомлений смены статуса быстрой оплаты");
		}

		protected override void OnStopService()
		{
			_logger.LogInformation("Остановлен сервис обработки уведомлений смены статуса быстрой оплаты");
		}
	}
}
