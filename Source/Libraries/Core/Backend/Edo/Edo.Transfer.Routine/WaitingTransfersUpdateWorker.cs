using Edo.Transfer.Routine.Options;
using Edo.Transfer.Routine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace Edo.Transfer.Routine
{
	public class WaitingTransfersUpdateWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<WaitingTransfersUpdateWorker> _logger;
		private readonly IOptionsMonitor<WaitingTransfersUpdateSettings> _optionsMonitor;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public WaitingTransfersUpdateWorker(
			ILogger<WaitingTransfersUpdateWorker> logger,
			IOptionsMonitor<WaitingTransfersUpdateSettings> optionsMonitor,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_optionsMonitor = optionsMonitor;
			_serviceScopeFactory = serviceScopeFactory;
		}
		protected override TimeSpan Interval => _optionsMonitor.CurrentValue.Interval;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using(var scope = _serviceScopeFactory.CreateScope())
			{
				var waitingTransfersUpdateService = scope.ServiceProvider.GetService<WaitingTransfersUpdateService>();

				_logger.LogInformation("Start waiting transfers update");

				await waitingTransfersUpdateService.Update(stoppingToken);

				_logger.LogInformation("End waiting transfers update");
			}

		}
	}
}
