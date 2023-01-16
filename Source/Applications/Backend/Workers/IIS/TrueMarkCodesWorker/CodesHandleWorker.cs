using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Models.TrueMark;
using Vodovoz.Services;
using Vodovoz.Settings.Edo;

namespace TrueMarkCodesWorker
{
	public class CodesHandleWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<CodesHandleWorker> _logger;
		private readonly IEdoSettings _edoSettings;
		private readonly TrueMarkCodesHandler _trueMarkCodesHandler;
		private readonly TimeSpan _interval;
		private bool _isRunning = false;

		public CodesHandleWorker(ILogger<CodesHandleWorker> logger, IEdoSettings edoSettings, TrueMarkCodesHandler trueMarkCodesHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_edoSettings = edoSettings ?? throw new ArgumentNullException(nameof(edoSettings));
			_trueMarkCodesHandler = trueMarkCodesHandler ?? throw new ArgumentNullException(nameof(trueMarkCodesHandler));
			_interval = TimeSpan.FromMinutes(_edoSettings.TrueMarkCodesHandleInterval);
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
				_logger.LogInformation("Вызов обработки кодов честного знака");
				await _trueMarkCodesHandler.HandleOrders(stoppingToken);
			}
			finally
			{
				_isRunning = false;
			}
		}

		protected override void OnStartService()
		{
			_logger.LogInformation("Запущен сервис обработки кодов честного знака");
		}

		protected override void OnStopService()
		{
			_logger.LogInformation("Остановлен сервис обработки кодов честного знака");
		}
	}
}
