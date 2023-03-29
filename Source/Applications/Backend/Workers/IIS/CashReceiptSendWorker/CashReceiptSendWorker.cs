using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Models.CashReceipts;
using Vodovoz.Models.TrueMark;

namespace CashReceiptSendWorker
{
	public class CashReceiptSendWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<CashReceiptSendWorker> _logger;
		private readonly CashReceiptsSender _cashReceiptsSender;
		private readonly StaleReceiptDocumentsRefresher _staleReceiptDocumentsRefresher;
		private readonly TimeSpan _standartInterval;
		private bool _isRunning = false;

		public CashReceiptSendWorker(
			ILogger<CashReceiptSendWorker> logger,
			CashReceiptsSender cashReceiptsSender,
			StaleReceiptDocumentsRefresher staleReceiptDocumentsRefresher
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_cashReceiptsSender = cashReceiptsSender ?? throw new ArgumentNullException(nameof(cashReceiptsSender));
			_staleReceiptDocumentsRefresher = staleReceiptDocumentsRefresher ?? throw new ArgumentNullException(nameof(staleReceiptDocumentsRefresher));
			_standartInterval = TimeSpan.FromSeconds(60);
		}
		protected override TimeSpan Interval
		{
			get
			{
				var now = DateTime.Now;
				if(now.Hour >= 1 && now.Hour < 5)
				{
					var newInterval = now.Date.AddHours(5) - now;
					_logger.LogInformation($"Отправка чеков отключена с 01:00 по 05:00. Следующий запуск в {now + newInterval}");
					return newInterval;
				}

				return _standartInterval;
			}
		}

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(_isRunning)
			{
				return;
			}

			_isRunning = true;

			try
			{
				_logger.LogInformation("Вызов отправки чеков");
				await _cashReceiptsSender.PrepareAndSendAsync(stoppingToken);

				_logger.LogInformation("Вызов обновления фискальных документов для чеков");
				await _staleReceiptDocumentsRefresher.RefreshDocuments(stoppingToken);
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
			_logger.LogInformation("Запущен сервис отправки чеков");
		}

		protected override void OnStopService()
		{
			_logger.LogInformation("Остановлен сервис отправки чеков");
		}
	}
}
