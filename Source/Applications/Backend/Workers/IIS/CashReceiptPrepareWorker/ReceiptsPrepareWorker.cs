using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Models.TrueMark;
using Vodovoz.Settings.Edo;

namespace CashReceiptPrepareWorker
{
	public class ReceiptsPrepareWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ReceiptsPrepareWorker> _logger;
		private readonly IEdoSettings _edoSettings;
		private readonly ReceiptsHandler _receiptsHandler;
		private readonly TimeSpan _interval;
		private bool _isRunning = false;

		public ReceiptsPrepareWorker(
			ILogger<ReceiptsPrepareWorker> logger,
			IEdoSettings edoSettings,
			ReceiptsHandler receiptsHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_receiptsHandler = receiptsHandler ?? throw new ArgumentNullException(nameof(receiptsHandler));
			_interval = TimeSpan.FromSeconds(_edoSettings.TrueMarkCodesHandleInterval);
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
				if(!_edoSettings.NewEdoProcessing)
				{
					_logger.LogInformation("Вызов обработки чеков");
					await _receiptsHandler.HandleReceiptsAsync(stoppingToken);

					_logger.LogInformation("Вызов создания чеков для самовывозов");
					await _receiptsHandler.CreateSelfDeliveryReceiptsAsync(stoppingToken);
				
					_logger.LogInformation("Вызов создания чеков для обычных заказов");
					await _receiptsHandler.CreateDeliveryOrderReceiptsAsync(stoppingToken);
				}
				else
				{
					_logger.LogInformation("Вызов обработки чеков");
					await _receiptsHandler.HandleReceiptsAsync(stoppingToken);

					_logger.LogInformation("Вызов обработки тасок чеков для самовывозов");
					await _receiptsHandler.HandleSelfDeliveryReceiptTasks(stoppingToken);
				
					_logger.LogInformation("Вызов создания тасок для обычных заказов");
					await _receiptsHandler.HandleDeliveryReceiptTasks(stoppingToken);
				}
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
			_logger.LogInformation("Запущен сервис обработки чеков");
		}

		protected override void OnStopService()
		{
			_logger.LogInformation("Остановлен сервис обработки чеков");
		}
	}
}
