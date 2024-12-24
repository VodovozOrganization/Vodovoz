using DeliveryRulesService.Cache;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace DeliveryRulesService.Workers
{
	public class DistrictCacheWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<DistrictCacheWorker> _logger;
		private readonly DistrictCacheService _districtCache;

		public DistrictCacheWorker(ILogger<DistrictCacheWorker> logger, DistrictCacheService districtCache)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_districtCache = districtCache
				?? throw new ArgumentNullException(nameof(districtCache));
		}

		protected override TimeSpan Interval => TimeSpan.FromHours(1);

		protected override Task DoWork(CancellationToken cancellationToken)
		{
			_districtCache.UpdateCache();
			return Task.CompletedTask;
		}

		protected override void OnStartService()
		{
			base.OnStartService();
			_logger.LogInformation("Процесс создания бэкапа районов запущен");
		}

		protected override void OnStopService()
		{
			base.OnStopService();
			_logger.LogInformation("Процесс создания бэкапа районов остановлен");
		}
	}
}
