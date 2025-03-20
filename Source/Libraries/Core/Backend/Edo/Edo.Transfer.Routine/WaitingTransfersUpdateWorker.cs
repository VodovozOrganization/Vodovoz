using Edo.Transfer.Routine.Options;
using Edo.Transfer.Routine.WaitingTransfersUpdate;
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
		private readonly IOptions<WaitingTransfersUpdateSettings> _options;
		private readonly WaitingTransfersUpdateService _waitingTransfersUpdateService;

		public WaitingTransfersUpdateWorker(
			ILogger<WaitingTransfersUpdateWorker> logger,
			IOptions<WaitingTransfersUpdateSettings> options,
			WaitingTransfersUpdateService waitingTransfersUpdateService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_waitingTransfersUpdateService = waitingTransfersUpdateService ?? throw new ArgumentNullException(nameof(waitingTransfersUpdateService));
		}
		protected override TimeSpan Interval => TimeSpan.FromSeconds(_options.Value.IntervalInSeconds);

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Start waiting transfers update");

			await _waitingTransfersUpdateService.Update(stoppingToken);

			_logger.LogInformation("End waiting transfers update");
		}
	}
}
