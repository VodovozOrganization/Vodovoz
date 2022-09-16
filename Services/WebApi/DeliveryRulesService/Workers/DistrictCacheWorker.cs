using DeliveryRulesService.Cache;
using Microsoft.Extensions.Logging;
using System;
using System.Timers;

namespace DeliveryRulesService.Workers
{
	public class DistrictCacheWorker : IDisposable
	{
		private const double _interval = 60 * 60 * 1000;    //1 час
		private readonly ILogger<DistrictCacheWorker> _logger;
		private readonly DistrictCache _districtCache;
		private Timer _timer;

		public DistrictCacheWorker(ILogger<DistrictCacheWorker> logger, DistrictCache districtCache)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_districtCache = districtCache ?? throw new ArgumentNullException(nameof(districtCache));
		}

		public void Start()
		{
			_logger.LogInformation("Запуск процесса создания бэкапа районов...");
			_timer = new Timer(_interval);
			_timer.Elapsed += TimerElapsed;
			_timer.Start();
			_districtCache.UpdateCache();
			_logger.LogInformation("Процесс создания бэкапа районов запущен");
		}

		public void Stop()
		{
			_logger.LogInformation("Запуск процесса создания бэкапа районов...");
			_timer.Stop();
			_timer.Elapsed -= TimerElapsed;
			_timer = null;
			_logger.LogInformation("Процесс создания бэкапа районов запущен");
		}

		private void TimerElapsed(object sender, ElapsedEventArgs e)
		{
			_districtCache.UpdateCache();
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
