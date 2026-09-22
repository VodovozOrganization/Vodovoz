using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vodovoz.Infrastructure;

namespace Edo.Receipt.Sender.Worker
{
	/// <summary>
	/// Периодически проверяет чеки, зависшие в очереди.
	/// </summary>
	public class ReceiptQueueNotificationWorker : TimerBackgroundServiceBase
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IOptionsMonitor<ReceiptQueueNotificationOptions> _options;
		private readonly ILogger<ReceiptQueueNotificationWorker> _logger;

		/// <summary>
		/// Создаёт обработчик уведомлений о зависших чеках.
		/// </summary>
		/// <param name="scopeFactory">Фабрика областей сервисов</param>
		/// <param name="options">Интервалы обработки</param>
		/// <param name="logger">Журнал событий</param>
		public ReceiptQueueNotificationWorker(
			IServiceScopeFactory scopeFactory,
			IOptionsMonitor<ReceiptQueueNotificationOptions> options,
			ILogger<ReceiptQueueNotificationWorker> logger)
		{
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc/>
		protected override TimeSpan Interval => _options.CurrentValue.WorkerInterval;

		/// <inheritdoc/>
		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				using var scope = _scopeFactory.CreateScope();
				var service = scope.ServiceProvider.GetRequiredService<IReceiptQueueNotificationService>();
				await service.ProcessAsync(DateTime.Now, stoppingToken);
			}
			catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
			{
				throw;
			}
			catch(Exception exception)
			{
				_logger.LogError(exception, "Ошибка проверки очереди чеков для уведомлений");
			}
		}
	}
}
