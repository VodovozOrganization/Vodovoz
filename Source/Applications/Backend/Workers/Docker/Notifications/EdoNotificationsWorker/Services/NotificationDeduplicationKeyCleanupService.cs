using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EdoNotificationsWorker.Services
{
	/// <summary>
	/// Удаляет устаревшие ключи дедупликации уведомлений (Notifications.Core.NotificationDeduplicationKey,
	/// таблица notification_deduplication_keys). Ключи пишут публикаторы уведомлений в транзакции уведомления —
	/// и десктоп, и сервисы. Таблица одна на всех, поэтому очистка работает в одном сервисе — здесь.
	/// </summary>
	public sealed class NotificationDeduplicationKeyCleanupService : BackgroundService
	{
		/// <summary>
		/// Сколько хранится ключ: в течение этого времени повторное уведомление не отправляется
		/// </summary>
		public static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(3);

		private static readonly TimeSpan _interval = TimeSpan.FromHours(1);
		private const int _batchSize = 10000;

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ILogger<NotificationDeduplicationKeyCleanupService> _logger;

		public NotificationDeduplicationKeyCleanupService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger<NotificationDeduplicationKeyCleanupService> logger)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				try
				{
					await CleanupAsync(stoppingToken);
				}
				catch(Exception ex) when(!stoppingToken.IsCancellationRequested)
				{
					_logger.LogError(ex, "Не удалось удалить устаревшие ключи дедупликации уведомлений");
				}

				try
				{
					await Task.Delay(_interval, stoppingToken);
				}
				catch(OperationCanceledException)
				{
					return;
				}
			}
		}

		private async Task CleanupAsync(CancellationToken cancellationToken)
		{
			// CreatedAt ключа пишется в UTC
			var olderThan = DateTime.UtcNow - RetentionPeriod;
			var total = 0;
			int deleted;

			// Пачками, каждая своей транзакцией, чтобы не держать долгие блокировки
			do
			{
				using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot("Очистка ключей дедупликации уведомлений"))
				{
					deleted = await unitOfWork.Session
						.CreateSQLQuery("DELETE FROM notification_deduplication_keys WHERE created_at < :olderThan LIMIT " + _batchSize)
						.SetParameter("olderThan", olderThan)
						.ExecuteUpdateAsync(cancellationToken);

					await unitOfWork.CommitAsync(cancellationToken);
				}

				total += deleted;
			}
			while(deleted == _batchSize && !cancellationToken.IsCancellationRequested);

			if(total > 0)
			{
				_logger.LogInformation("Удалено устаревших ключей дедупликации уведомлений: {Count}", total);
			}
		}
	}
}
